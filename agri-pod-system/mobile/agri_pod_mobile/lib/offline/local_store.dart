import 'dart:convert';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';
import 'package:sqflite/sqflite.dart';
import 'package:uuid/uuid.dart';

class LocalStore {
  Database? _db;

  Future<void> open() async {
    final dir = await getApplicationDocumentsDirectory();
    _db = await openDatabase(
      p.join(dir.path, 'agri_pod_mobile.db'),
      version: 1,
      onCreate: (db, version) async {
        await db.execute('''
          create table pending_mutations(
            id text primary key,
            entity_name text not null,
            operation text not null,
            json_payload text not null,
            occurred_at text not null,
            synced_at text
          )
        ''');
      },
    );
  }

  Future<void> queueMutation({
    required String entityName,
    required String operation,
    required Map<String, Object?> payload,
  }) async {
    await _database.insert('pending_mutations', {
      'id': const Uuid().v4(),
      'entity_name': entityName,
      'operation': operation,
      'json_payload': jsonEncode(payload),
      'occurred_at': DateTime.now().toUtc().toIso8601String(),
    });
  }

  Future<List<Map<String, Object?>>> pendingMutations() {
    return _database.query(
      'pending_mutations',
      where: 'synced_at is null',
      orderBy: 'occurred_at',
    );
  }

  Future<int> pendingMutationCount() async {
    final result = await _database.rawQuery(
      'select count(*) as count from pending_mutations where synced_at is null',
    );
    return result.first['count'] as int;
  }

  Future<void> markSynced(List<String> ids) async {
    if (ids.isEmpty) {
      return;
    }

    final placeholders = List.filled(ids.length, '?').join(',');
    await _database.rawUpdate(
      'update pending_mutations set synced_at = ? where id in ($placeholders)',
      [DateTime.now().toUtc().toIso8601String(), ...ids],
    );
  }

  Database get _database {
    final database = _db;
    if (database == null) {
      throw StateError('LocalStore.open must be called before use.');
    }

    return database;
  }
}
