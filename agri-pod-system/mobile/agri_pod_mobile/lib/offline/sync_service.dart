import 'dart:convert';

import 'package:agri_pod_mobile/offline/local_store.dart';
import 'package:http/http.dart' as http;

class SyncService {
  SyncService(this._store, {http.Client? httpClient})
      : _httpClient = httpClient ?? http.Client();

  final LocalStore _store;
  final http.Client _httpClient;
  final Uri _apiBaseUri = Uri.parse('http://localhost:5068');

  Future<int> syncPending() async {
    final pending = await _store.pendingMutations();
    if (pending.isEmpty) {
      return 0;
    }

    final acceptedIds = <String>[];
    final batchMutations = <Map<String, Object?>>[];

    for (final row in pending) {
      final entityName = row['entity_name'] as String;
      final operation = row['operation'] as String;
      final payload = row['json_payload'] as String;

      if (entityName == 'Farmer' && operation == 'Register') {
        final response = await _postJson('/api/v1/farmers', payload);
        if (response >= 200 && response < 300) {
          acceptedIds.add(row['id'] as String);
        }
      } else {
        batchMutations.add({
          'clientMutationId': row['id'],
          'entityName': entityName,
          'operation': operation,
          'jsonPayload': payload,
          'occurredAt': row['occurred_at'],
        });
      }
    }

    if (batchMutations.isNotEmpty) {
      final response = await _postJson(
        '/api/v1/sync',
        jsonEncode({
          'deviceId': 'SL-FIELD-DEVICE',
          'districtCode': 'WESTERN',
          'lastServerVersion': 0,
          'mutations': batchMutations,
        }),
      );

      if (response >= 200 && response < 300) {
        acceptedIds.addAll(batchMutations.map((row) => row['clientMutationId'] as String));
      }
    }

    await _store.markSynced(acceptedIds);
    return acceptedIds.length;
  }

  Future<int> _postJson(String path, String jsonBody) async {
    try {
      final response = await _httpClient.post(
        _apiBaseUri.replace(path: path),
        headers: {'content-type': 'application/json'},
        body: jsonBody,
      );
      return response.statusCode;
    } catch (_) {
      return 0;
    }
  }
}
