import 'dart:async';
import 'dart:io';

import 'package:flutter/foundation.dart';
import 'package:geolocator/geolocator.dart';
import 'package:path_provider/path_provider.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:record/record.dart';

import 'package:oliejournal_app/backend.dart';

class VoiceRecordingController extends ChangeNotifier {
  final AudioRecorder _audioRecorder = AudioRecorder();
  Timer? _timer;

  int _secondsRemaining = 55;
  bool _isRecording = false;
  bool _isUploading = false;
  String? _recordingPath;
  String _recordingFinished = 'Last recording saved! Start a new recording.';

  int get secondsRemaining => _secondsRemaining;
  bool get isRecording => _isRecording;
  bool get isUploading => _isUploading;
  String? get recordingPath => _recordingPath;
  String get recordingFinished => _recordingFinished;

  Future<List<String>> requestPermissions() async {
    final warnings = <String>[];

    final microphoneStatus = await Permission.microphone.request();
    if (!microphoneStatus.isGranted) {
      warnings.add('Microphone permission is required to record audio.');
    }

    var locationPermission = await Geolocator.checkPermission();
    if (locationPermission == LocationPermission.denied) {
      locationPermission = await Geolocator.requestPermission();
    }

    if (locationPermission == LocationPermission.denied ||
        locationPermission == LocationPermission.deniedForever) {
      warnings.add(
        'Enable location permission to record your journaling location.',
      );
    }

    return warnings;
  }

  Future<void> startRecording() async {
    if (!await _audioRecorder.hasPermission()) {
      throw Exception('Microphone permission is required to record audio.');
    }

    final fileName =
        'oliejournal_${DateTime.now().toUtc().millisecondsSinceEpoch}.wav';
    final tempDirectory = await getTemporaryDirectory();
    final outputPath = '${tempDirectory.path}/$fileName';

    await _audioRecorder.start(
      const RecordConfig(
        numChannels: 1,
        encoder: AudioEncoder.wav,
        autoGain: true,
        sampleRate: 16000,
        noiseSuppress: true,
      ),
      path: outputPath,
    );

    _isRecording = true;
    _secondsRemaining = 55;
    _recordingPath = outputPath;
    notifyListeners();

    _startTimer();
  }

  Future<String?> stopRecording() async {
    _timer?.cancel();

    final path = await _audioRecorder.stop();

    _isRecording = false;
    _secondsRemaining = 55;
    _recordingPath = path;
    notifyListeners();

    return path;
  }

  void markRecordingNotSaved() {
    _recordingFinished = 'Recording was not saved! Start a new recording.';
    notifyListeners();
  }

  Future<void> uploadRecording({
    required String path,
    required String? token,
  }) async {
    if (token == null) {
      throw Exception('Not authenticated; please log in before uploading.');
    }

    _isUploading = true;
    _recordingFinished = 'Don\'t leave this page. Obtaining your location.';
    notifyListeners();

    Position? position;
    try {
      position = await _determinePosition();
    } catch (e) {
      debugPrint('Location error: $e');
    }

    _recordingFinished = 'Don\'t leave this page. Uploading now!';
    notifyListeners();

    try {
      final file = File(path);
      await Backend.uploadAudioEntry(
        token,
        file,
        latitude: position?.latitude,
        longitude: position?.longitude,
        onRetryScheduled: (retryInfo) {
          _recordingFinished =
              'Weak reception detected. Waiting for cell reception '
              'and retrying upload (${retryInfo.nextAttempt}/${retryInfo.maxAttempts}) '
              'in ${retryInfo.retryDelay.inSeconds}s.';
          notifyListeners();
        },
      );

      _isUploading = false;
      _recordingFinished = 'Recording uploaded!';
      notifyListeners();
    } catch (e) {
      final message =
          'Unfortunately, your recording could not be saved. Please record a new entry to try again.\n\n$e';
      _isUploading = false;
      _recordingFinished = message;
      notifyListeners();
      throw Exception(message);
    }
  }

  void _startTimer() {
    _timer?.cancel();
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      _secondsRemaining--;
      notifyListeners();

      if (_secondsRemaining <= 0) {
        stopRecording();
      }
    });
  }

  Future<Position> _determinePosition() async {
    var serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) {
      throw Exception('Location services are disabled.');
    }

    var permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
      if (permission == LocationPermission.denied) {
        throw Exception('Location permissions are denied');
      }
    }

    if (permission == LocationPermission.deniedForever) {
      throw Exception('Location permissions are permanently denied');
    }

    return Geolocator.getCurrentPosition();
  }

  @override
  void dispose() {
    _timer?.cancel();
    _audioRecorder.dispose();
    super.dispose();
  }
}
