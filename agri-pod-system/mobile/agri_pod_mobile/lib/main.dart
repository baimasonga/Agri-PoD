import 'package:agri_pod_mobile/offline/local_store.dart';
import 'package:agri_pod_mobile/offline/sync_service.dart';
import 'package:flutter/material.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  final store = LocalStore();
  await store.open();
  runApp(AgriPodMobile(store: store, syncService: SyncService(store)));
}

class AgriPodMobile extends StatelessWidget {
  const AgriPodMobile({
    required this.store,
    required this.syncService,
    super.key,
  });

  final LocalStore store;
  final SyncService syncService;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Agri-PoD Field',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xffc2410c),
          brightness: Brightness.dark,
          primary: const Color(0xffea580c),
          surface: const Color(0xff0f172a),
          surfaceContainer: const Color(0xff1e293b),
        ),
        useMaterial3: true,
        scaffoldBackgroundColor: const Color(0xff0f172a),
        appBarTheme: const AppBarTheme(
          centerTitle: false,
          elevation: 0,
          backgroundColor: Colors.transparent,
          titleTextStyle: TextStyle(
            color: Colors.white,
            fontSize: 24,
            fontWeight: FontWeight.w900,
            letterSpacing: -1.0,
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: const Color(0xff1e293b),
          labelStyle: const TextStyle(color: Color(0xff94a3b8), fontWeight: FontWeight.w600),
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(20),
            borderSide: BorderSide.none,
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(20),
            borderSide: const BorderSide(color: Color(0xffea580c), width: 2),
          ),
          contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 20),
        ),
        navigationBarTheme: NavigationBarThemeData(
          elevation: 0,
          backgroundColor: const Color(0xff1e293b),
          indicatorColor: const Color(0xffea580c).withValues(alpha: 0.2),
          labelTextStyle: WidgetStateProperty.resolveWith((states) {
            if (states.contains(WidgetState.selected)) {
              return const TextStyle(fontWeight: FontWeight.w800, color: Color(0xffea580c), fontSize: 12);
            }
            return const TextStyle(fontWeight: FontWeight.w500, color: Color(0xff94a3b8), fontSize: 12);
          }),
        ),
      ),
      home: FieldOfficerShell(store: store, syncService: syncService),
    );
  }
}

class FieldOfficerShell extends StatefulWidget {
  const FieldOfficerShell({
    required this.store,
    required this.syncService,
    super.key,
  });

  final LocalStore store;
  final SyncService syncService;

  @override
  State<FieldOfficerShell> createState() => _FieldOfficerShellState();
}

class _FieldOfficerShellState extends State<FieldOfficerShell> {
  int _index = 0;
  int _pendingMutations = 0;

  @override
  void initState() {
    super.initState();
    _refreshPending();
  }

  Future<void> _refreshPending() async {
    final count = await widget.store.pendingMutationCount();
    setState(() => _pendingMutations = count);
  }

  Future<void> _sync() async {
    final count = await widget.syncService.syncPending();
    await _refreshPending();
    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          behavior: SnackBarBehavior.floating,
          backgroundColor: const Color(0xffea580c),
          content: Text('Synced $count records to National Hub', style: const TextStyle(fontWeight: FontWeight.w800)),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final pages = [
      FarmerRegistrationPage(store: widget.store, onSaved: _refreshPending),
      SessionPage(store: widget.store, onSaved: _refreshPending),
      DeliveryCapturePage(store: widget.store, onSaved: _refreshPending),
    ];

    return Scaffold(
      appBar: AppBar(
        title: const Text('FIELD COMMAND'),
        actions: [
          Padding(
            padding: const EdgeInsets.only(right: 16.0),
            child: IconButton.filledTonal(
              onPressed: _sync,
              icon: Badge(
                label: Text('$_pendingMutations'),
                isLabelVisible: _pendingMutations > 0,
                child: const Icon(Icons.sync_rounded),
              ),
            ),
          ),
        ],
      ),
      body: pages[_index],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.person_pin_rounded), label: 'Register'),
          NavigationDestination(icon: Icon(Icons.hub_rounded), label: 'Session'),
          NavigationDestination(icon: Icon(Icons.inventory_rounded), label: 'PoD'),
        ],
      ),
    );
  }
}

class FarmerRegistrationPage extends StatefulWidget {
  const FarmerRegistrationPage({
    required this.store,
    required this.onSaved,
    super.key,
  });

  final LocalStore store;
  final Future<void> Function() onSaved;

  @override
  State<FarmerRegistrationPage> createState() => _FarmerRegistrationPageState();
}

class _FarmerRegistrationPageState extends State<FarmerRegistrationPage> {
  final _name = TextEditingController();
  final _nationalId = TextEditingController();
  final _phone = TextEditingController();
  final _community = TextEditingController();
  String _status = 'Standby. Awaiting farmer registration data.';

  Future<void> _save() async {
    if (_name.text.isEmpty) return;
    await widget.store.queueMutation(
      entityName: 'Farmer',
      operation: 'Register',
      payload: {
        'fullName': _name.text,
        'nationalId': _nationalId.text,
        'phone': _phone.text,
        'districtCode': 'BOMBALI',
        'community': _community.text,
        'valueChain': 'Rice',
        'latitude': 8.889,
        'longitude': -12.044,
        'photoReference': 'offline_img_ref',
      },
    );
    await widget.onSaved();
    setState(() {
      _status = 'Success. Farmer record encrypted and queued.';
      _name.clear();
      _nationalId.clear();
      _phone.clear();
      _community.clear();
    });
  }

  @override
  Widget build(BuildContext context) {
    return WorkflowForm(
      title: 'Beneficiary Onboarding',
      status: _status,
      actionLabel: 'Enroll Farmer',
      onPressed: _save,
      children: [
        TextField(controller: _name, decoration: const InputDecoration(labelText: 'Full legal name')),
        TextField(controller: _nationalId, decoration: const InputDecoration(labelText: 'National Identity Number')),
        TextField(controller: _phone, decoration: const InputDecoration(labelText: 'Contact Phone')),
        TextField(controller: _community, decoration: const InputDecoration(labelText: 'Primary Community')),
      ],
    );
  }
}

class SessionPage extends StatefulWidget {
  const SessionPage({
    required this.store,
    required this.onSaved,
    super.key,
  });

  final LocalStore store;
  final Future<void> Function() onSaved;

  @override
  State<SessionPage> createState() => _SessionPageState();
}

class _SessionPageState extends State<SessionPage> {
  final _manifest = TextEditingController();
  String _status = 'Scan or enter the vehicle manifest code.';

  Future<void> _start() async {
    if (_manifest.text.isEmpty) return;
    await widget.store.queueMutation(
      entityName: 'DistributionSession',
      operation: 'Start',
      payload: {
        'manifestBarcode': _manifest.text,
        'fieldOfficerUserId': 'field-user-01',
        'latitude': 8.484,
        'longitude': -13.229,
        'startedAt': DateTime.now().toUtc().toIso8601String(),
      },
    );
    await widget.onSaved();
    setState(() => _status = 'Session established. Ready for PoD capture.');
  }

  @override
  Widget build(BuildContext context) {
    return WorkflowForm(
      title: 'Distribution Session',
      status: _status,
      actionLabel: 'Establish Site Node',
      onPressed: _start,
      children: [
        TextField(controller: _manifest, decoration: const InputDecoration(labelText: 'Manifest Barcode', prefixIcon: Icon(Icons.qr_code_2_rounded))),
      ],
    );
  }
}

class DeliveryCapturePage extends StatefulWidget {
  const DeliveryCapturePage({
    required this.store,
    required this.onSaved,
    super.key,
  });

  final LocalStore store;
  final Future<void> Function() onSaved;

  @override
  State<DeliveryCapturePage> createState() => _DeliveryCapturePageState();
}

class _DeliveryCapturePageState extends State<DeliveryCapturePage> {
  final _farmerBarcode = TextEditingController();
  final _packageBarcode = TextEditingController();
  final _otp = TextEditingController();
  final _quantity = TextEditingController();
  String _status = 'Ready for Proof of Delivery evidence capture.';

  Future<void> _captureProof() async {
    if (_farmerBarcode.text.isEmpty) return;
    await widget.store.queueMutation(
      entityName: 'ProofOfDelivery',
      operation: 'Confirm',
      payload: {
        'farmerBarcode': _farmerBarcode.text,
        'packageBarcode': _packageBarcode.text,
        'otpCode': _otp.text,
        'faceCaptureReference': 'offline_face_ref',
        'latitude': 8.484,
        'longitude': -13.229,
        'vehicleRegistration': 'SL-AG-104',
        'quantity': double.tryParse(_quantity.text) ?? 1,
        'capturedByUserId': 'field-user-01',
        'timestamp': DateTime.now().toUtc().toIso8601String(),
        'offlineTransactionId': 'offline_txn_${DateTime.now().millisecondsSinceEpoch}',
      },
    );

    await widget.onSaved();
    setState(() => _status = 'Evidence captured and secured locally.');
    _farmerBarcode.clear(); _packageBarcode.clear(); _otp.clear(); _quantity.clear();
  }

  @override
  Widget build(BuildContext context) {
    return WorkflowForm(
      title: 'Proof of Delivery',
      status: _status,
      actionLabel: 'Confirm Handover',
      onPressed: _captureProof,
      children: [
        TextField(controller: _farmerBarcode, decoration: const InputDecoration(labelText: 'Farmer Barcode')),
        TextField(controller: _packageBarcode, decoration: const InputDecoration(labelText: 'Input Package Barcode')),
        TextField(controller: _otp, decoration: const InputDecoration(labelText: 'Verification OTP')),
        TextField(controller: _quantity, decoration: const InputDecoration(labelText: 'Quantity Issued')),
      ],
    );
  }
}

class WorkflowForm extends StatelessWidget {
  const WorkflowForm({
    required this.title,
    required this.status,
    required this.actionLabel,
    required this.onPressed,
    required this.children,
    super.key,
  });

  final String title;
  final String status;
  final String actionLabel;
  final Future<void> Function() onPressed;
  final List<Widget> children;

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
      children: [
        Text(
          title, 
          style: const TextStyle(
            fontSize: 36,
            fontWeight: FontWeight.w900,
            color: Colors.white,
            letterSpacing: -1.5,
            height: 1.0,
          ),
        ),
        const SizedBox(height: 12),
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: const Color(0xffea580c).withValues(alpha: 0.1),
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: const Color(0xffea580c).withValues(alpha: 0.3)),
          ),
          child: Text(
            status,
            style: const TextStyle(color: Color(0xfffb923c), fontWeight: FontWeight.w700, fontSize: 13),
          ),
        ),
        const SizedBox(height: 32),
        ...children.expand((child) => [child, const SizedBox(height: 20)]),
        const SizedBox(height: 12),
        Container(
          height: 72,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(24),
            boxShadow: [
              BoxShadow(
                color: const Color(0xffea580c).withValues(alpha: 0.3),
                blurRadius: 20,
                offset: const Offset(0, 8),
              )
            ]
          ),
          child: ElevatedButton(
            style: ElevatedButton.styleFrom(
              backgroundColor: const Color(0xffea580c),
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(24)),
              elevation: 0,
            ),
            onPressed: onPressed,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(actionLabel, style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 18, letterSpacing: -0.5)),
                const SizedBox(width: 12),
                const Icon(Icons.arrow_forward_ios_rounded, size: 16),
              ],
            ),
          ),
        ),
      ],
    );
  }
}
