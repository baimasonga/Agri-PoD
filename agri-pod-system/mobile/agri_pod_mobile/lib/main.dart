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
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xff226b45)),
        useMaterial3: true,
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
    await widget.syncService.syncPending();
    await _refreshPending();
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
    setState(() => _status = 'Farmer registration queued for district review.');
  }

  @override
  Widget build(BuildContext context) {
    return WorkflowForm(
      title: 'Farmer registration',
      status: _status,
      actionLabel: 'Save farmer offline',
      onPressed: _save,
      children: [
        TextField(controller: _name, decoration: const InputDecoration(labelText: 'Full name')),
        TextField(controller: _nationalId, decoration: const InputDecoration(labelText: 'National ID')),
        TextField(controller: _phone, decoration: const InputDecoration(labelText: 'Phone')),
        TextField(controller: _community, decoration: const InputDecoration(labelText: 'Community')),
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
        TextField(controller: _manifest, decoration: const InputDecoration(labelText: 'Manifest barcode')),
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
  String _status = 'PoD requires barcode, OTP, face reference, GPS, and vehicle proximity.';

  @override
  void dispose() {
    _farmerBarcode.dispose();
    _packageBarcode.dispose();
    _otp.dispose();
    _quantity.dispose();
    super.dispose();
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
        TextField(controller: _farmerBarcode, decoration: const InputDecoration(labelText: 'Farmer barcode')),
        TextField(controller: _packageBarcode, decoration: const InputDecoration(labelText: 'Package barcode')),
        TextField(controller: _otp, decoration: const InputDecoration(labelText: 'OTP code')),
        TextField(controller: _quantity, decoration: const InputDecoration(labelText: 'Quantity delivered')),
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
      padding: const EdgeInsets.all(16),
      children: [
        Text(title, style: Theme.of(context).textTheme.headlineSmall),
        const SizedBox(height: 12),
        ...children.expand((child) => [child, const SizedBox(height: 12)]),
        FilledButton.icon(
          onPressed: onPressed,
          icon: const Icon(Icons.offline_pin),
          label: Text(actionLabel),
        ),
        const SizedBox(height: 16),
        Card(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Text(status),
          ),
        ),
      ],
    );
  }
}
