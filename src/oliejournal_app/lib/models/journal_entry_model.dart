import 'package:intl/intl.dart';

class JournalEntryModel {
  final int id;
  final String userId;
  final DateTime created;
  final String? transcript;
  final String? responsePath;
  final String? responseText;

  const JournalEntryModel({
    required this.id,
    required this.userId,
    required this.created,
    required this.transcript,
    required this.responsePath,
    required this.responseText
  });

  factory JournalEntryModel.fromJson(Map<String, dynamic> json) {
    final format = DateFormat("yyyy-MM-dd'T'HH:mm:ss");
    
    return JournalEntryModel(
      id: json['id'] as int,
      userId: json['userId'] as String,
      created: format.parse(json['created']),
      transcript: json['transcript'] as String?,
      responsePath: json['responsePath'] as String?,
      responseText: json['responseText'] as String?
    );
  }
}
