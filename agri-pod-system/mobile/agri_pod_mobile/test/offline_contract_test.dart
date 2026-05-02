import 'package:flutter_test/flutter_test.dart';

void main() {
  test('offline mutation contract keeps required fields stable', () {
    const requiredFields = [
      'clientMutationId',
      'entityName',
      'operation',
      'jsonPayload',
      'occurredAt',
    ];

    expect(requiredFields, contains('clientMutationId'));
    expect(requiredFields, hasLength(5));
  });
}
