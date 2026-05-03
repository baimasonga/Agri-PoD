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
      title: 'Agri-PoD Mobile',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(
          seedColor: const Color(0xff10b981),
          brightness: Brightness.light,
          primary: const Color(0xff059669),
          surface: const Color(0xfff0fdf4),
        ),
        useMaterial3: true,
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
          scrolledUnderElevation: 0,
          backgroundColor: Colors.transparent,
          titleTextStyle: TextStyle(
            color: Color(0xff064e3b),
            fontSize: 20,
            fontWeight: FontWeight.w800,
            letterSpacing: -0.5,
          ),
          iconTheme: IconThemeData(color: Color(0xff059669)),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: Colors.white,
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: BorderSide.none,
          ),
          enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: BorderSide.none,
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: Color(0xff10b981), width: 2),
          ),
          contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 16),
        ),
        navigationBarTheme: NavigationBarThemeData(
          elevation: 0,
          backgroundColor: Colors.white,
          indicatorColor: const Color(0xffd1fae5),
          labelTextStyle: WidgetStateProperty.resolveWith((states) {
            if (states.contains(WidgetState.selected)) {
              return const TextStyle(fontWeight: FontWeight.w700, color: Color(0xff059669), fontSize: 12);
            }
            return const TextStyle(fontWeight: FontWeight.w500, color: Colors.grey, fontSize: 12);
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
        SnackBar(content: Text('Successfully synced $count items to national portal.')),
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
        title: const Text('Agri-PoD field app'),
        actions: [
          TextButton.icon(
            onPressed: _sync,
            icon: const Icon(Icons.sync),
            label: Text('$_pendingMutations'),
          ),
        ],
      ),
      body: pages[_index],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (value) => setState(() => _index = value),
        destinations: const [
          NavigationDestination(icon: Icon(Icons.person_add_alt), label: 'Farmer'),
          NavigationDestination(icon: Icon(Icons.local_shipping), label: 'Session'),
          NavigationDestination(icon: Icon(Icons.verified), label: 'PoD'),
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
  final _name = TextEditingController(text: 'Aminata Kamara');
  final _nationalId = TextEditingController(text: 'SL-NIN-00042');
  final _phone = TextEditingController(text: '+23276000000');
  final _community = TextEditingController(text: 'Makeni');
  String _status = 'Registration saves locally first.';

  @override
  void dispose() {
    _name.dispose();
    _nationalId.dispose();
    _phone.dispose();
    _community.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    await widget.store.queueMutation(
      entityName: 'Farmer',
      operation: 'Register',
      payload: {
        'fullName': _name.text,
        'nationalId': _nationalId.text,
        'phone': _phone.text,
        'districtCode': 'BOMBALI',
        'chiefdom': 'Bombali Sebora',
        'community': _community.text,
        'valueChain': 'Rice',
        'latitude': 8.889,
        'longitude': -12.044,
        'photoReference': 'local-photo://farmer/latest',
      },
    );
    await widget.onSaved();
    setState(() {
      _status = 'Farmer registration queued for district review.';
      _name.clear();
      _nationalId.clear();
      _phone.clear();
      _community.clear();
    });
  }

  @override
  Widget build(BuildContext context) {
    return WorkflowForm(
      title: 'Farmer registration',
      status: _status,
      actionLabel: 'Save farmer offline',
      onPressed: _save,
      children: [
        TextField(controller: _name, decoration: const InputDecoration(labelText: 'Full name', prefixIcon: Icon(Icons.person))),
        TextField(controller: _nationalId, decoration: const InputDecoration(labelText: 'National ID', prefixIcon: Icon(Icons.badge))),
        TextField(controller: _phone, decoration: const InputDecoration(labelText: 'Phone', prefixIcon: Icon(Icons.phone))),
        TextField(controller: _community, decoration: const InputDecoration(labelText: 'Community', prefixIcon: Icon(Icons.location_city))),
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
  final _manifest = TextEditingController(text: 'MANIFEST-SL-2026-001');
  String _status = 'Scan vehicle manifest at the distribution site.';

  @override
  void dispose() {
    _manifest.dispose();
    super.dispose();
  }

  Future<void> _start() async {
    await widget.store.queueMutation(
      entityName: 'DistributionSession',
      operation: 'Start',
      payload: {
        'manifestBarcode': _manifest.text,
        'fieldOfficerUserId': 'field-officer-01',
        'latitude': 8.484,
        'longitude': -13.229,
        'startedAt': DateTime.now().toUtc().toIso8601String(),
      },
    );
    await widget.onSaved();
    setState(() => _status = 'Session start queued with GPS validation evidence.');
  }

  @override
  Widget build(BuildContext context) {
    return WorkflowForm(
      title: 'Distribution session',
      status: _status,
      actionLabel: 'Start session offline',
      onPressed: _start,
      children: [
        TextField(controller: _manifest, decoration: const InputDecoration(labelText: 'Manifest barcode', prefixIcon: Icon(Icons.qr_code_scanner))),
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
  final _farmerBarcode = TextEditingController(text: 'FARMER-LOCAL-001');
  final _packageBarcode = TextEditingController(text: 'PKG-NPK-0001');
  final _otp = TextEditingController(text: '123456');
  final _quantity = TextEditingController(text: '2');
  final _signature = TextEditingController(text: 'Farmer Signature');
  String _status = 'PoD requires barcode, OTP, face reference, GPS, and vehicle proximity.';

  @override
  void dispose() {
    _farmerBarcode.dispose();
    _packageBarcode.dispose();
    _otp.dispose();
    _quantity.dispose();
    _signature.dispose();
    super.dispose();
  }

  void _requestTwilioOtp() {
    setState(() => _status = 'Requesting OTP via Twilio... (Requires Network)');
    Future.delayed(const Duration(seconds: 1), () {
      if (mounted) setState(() => _status = 'Twilio OTP SMS sent to farmer.');
    });
  }

  Future<void> _captureProof() async {
    await widget.store.queueMutation(
      entityName: 'ProofOfDelivery',
      operation: 'Confirm',
      payload: {
        'farmerBarcode': _farmerBarcode.text,
        'packageBarcode': _packageBarcode.text,
        'otpCode': _otp.text,
        'faceCaptureReference': 'local-face://capture/latest',
        'latitude': 8.484,
        'longitude': -13.229,
        'vehicleRegistration': 'SL-AG-104',
        'quantity': double.tryParse(_quantity.text) ?? 0,
        'capturedByUserId': 'field-officer-01',
        'timestamp': DateTime.now().toUtc().toIso8601String(),
        'signatureReference': _signature.text.isEmpty ? null : _signature.text,
        'photoEvidenceReference': 'local-photo://evidence/latest',
        'offlineTransactionId': 'txn-${DateTime.now().millisecondsSinceEpoch}',
      },
    );

    await widget.onSaved();
    setState(() => _status = 'PoD saved locally. It will sync when connectivity returns.');
  }

  @override
  Widget build(BuildContext context) {
    return WorkflowForm(
      title: 'Proof of delivery',
      status: _status,
      actionLabel: 'Save PoD offline',
      onPressed: _captureProof,
      children: [
        TextField(controller: _farmerBarcode, decoration: const InputDecoration(labelText: 'Farmer barcode', prefixIcon: Icon(Icons.qr_code))),
        TextField(controller: _packageBarcode, decoration: const InputDecoration(labelText: 'Package barcode', prefixIcon: Icon(Icons.inventory_2))),
        Row(
          children: [
            Expanded(child: TextField(controller: _otp, decoration: const InputDecoration(labelText: 'OTP code', prefixIcon: Icon(Icons.password)))),
            const SizedBox(width: 8),
            FilledButton.tonal(
              onPressed: _requestTwilioOtp,
              child: const Text('Twilio SMS'),
            ),
          ],
        ),
        TextField(controller: _quantity, decoration: const InputDecoration(labelText: 'Quantity delivered', prefixIcon: Icon(Icons.scale))),
        TextField(controller: _signature, decoration: const InputDecoration(labelText: 'Signature / Thumbprint (Optional)', prefixIcon: Icon(Icons.draw))),
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
      padding: const EdgeInsets.all(24),
      children: [
        Text(
          title, 
          style: Theme.of(context).textTheme.headlineMedium?.copyWith(
            fontWeight: FontWeight.w800,
            color: const Color(0xff064e3b),
            letterSpacing: -0.5,
          )
        ),
        const SizedBox(height: 6),
        Text(
          'Fill out the details below to queue locally.',
          style: TextStyle(color: Colors.grey.shade600, fontSize: 14),
        ),
        const SizedBox(height: 24),
        Container(
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(24),
            boxShadow: [
              BoxShadow(
                color: const Color(0xff10b981).withValues(alpha: 0.08),
                blurRadius: 24,
                offset: const Offset(0, 8),
              )
            ]
          ),
          padding: const EdgeInsets.all(24),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              ...children.expand((child) => [child, const SizedBox(height: 16)]),
              const SizedBox(height: 8),
              FilledButton.icon(
                style: FilledButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
                  backgroundColor: Theme.of(context).colorScheme.primary,
                  elevation: 0,
                ),
                onPressed: onPressed,
                icon: const Icon(Icons.cloud_upload_outlined),
                label: Text(actionLabel, style: const TextStyle(fontWeight: FontWeight.w700, fontSize: 16)),
              ),
            ],
          ),
        ),
        const SizedBox(height: 24),
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: const Color(0xffecfdf5),
            borderRadius: BorderRadius.circular(16),
            border: Border.all(color: const Color(0xffa7f3d0)),
          ),
          child: Row(
            children: [
              const Icon(Icons.info_outline, color: Color(0xff059669)),
              const SizedBox(width: 12),
              Expanded(
                child: Text(
                  status,
                  style: const TextStyle(color: Color(0xff065f46), fontWeight: FontWeight.w500, height: 1.4),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
