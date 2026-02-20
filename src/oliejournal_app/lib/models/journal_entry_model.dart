class JournalEntryModel {
  final int id;
  final String userId;
  final DateTime created;
  final String? transcript;
  final String? responsePath;
  final String? responseText;
  final double? latitude;
  final double? longitude;

  const JournalEntryModel({
    required this.id,
    required this.userId,
    required this.created,
    required this.transcript,
    required this.responsePath,
    required this.responseText,
    required this.latitude,
    required this.longitude,
  });

  factory JournalEntryModel.fromJson(Map<String, dynamic> json) {
    String? responsePath = json['responsePath'] as String?;
    if (responsePath != null) {
      responsePath = 'https://oliejournal.olievortex.com/videos/$responsePath';
    }

    return JournalEntryModel(
      id: json['id'] as int,
      userId: json['userId'] as String,
      created: DateTime.parse(json['created']),
      transcript: json['transcript'] as String?,
      responsePath: responsePath,
      responseText: json['responseText'] as String?,
      latitude: json['latitude'] as double?,
      longitude: json['longitude'] as double?,
    );
  }
}
