import 'dart:async';
import 'package:flutter/material.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:path_provider/path_provider.dart';
import 'package:record/record.dart';
import 'dart:io';
import 'package:provider/provider.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/backend.dart';
import 'package:geolocator/geolocator.dart';

class VoiceRecordingPage extends StatefulWidget {
  const VoiceRecordingPage({super.key});

  @override
  State<VoiceRecordingPage> createState() => _VoiceRecordingPageState();
}

class _VoiceRecordingPageState extends State<VoiceRecordingPage> {
  final AudioRecorder _audioRecorder = AudioRecorder();
  Timer? _timer;
  int _secondsRemaining = 55;
  bool _isRecording = false;
  bool _isUploading = false;
  String? _recordingPath;
  String _recordingFinished = 'Last recording saved! Start a new recording.';

  void _setStateIfMounted(VoidCallback fn) {
    if (!mounted) {
      return;
    }
    setState(fn);
  }

  @override
  void initState() {
    super.initState();
    _fetchPermission();
  }

  @override
  void dispose() {
    _timer?.cancel();
    _audioRecorder.dispose();
    super.dispose();
  }

  Future<void> _fetchPermission() async {
    await _requestMicrophonePermission();
    await _requestPositionPermission();
  }

  Future<void> _requestMicrophonePermission() async {
    final status = await Permission.microphone.request();
    if (!status.isGranted) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Microphone permission is required to record audio.'),
          ),
        );
      }
    }
  }

  Future<void> _requestPositionPermission() async {
    LocationPermission permission;

    permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
    }

    if (permission == LocationPermission.denied || permission == LocationPermission.deniedForever) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Enable location permission to record your journaling location.'),
          ),
        );
      }
    }
  }

  Future<void> _startRecording() async {
    try {
      if (await _audioRecorder.hasPermission()) {
        // Generate a file path for the recording
        final fileName =
            'oliejournal_${DateTime.now().toUtc().millisecondsSinceEpoch}.wav';
        final tempDirectory = await getTemporaryDirectory();
        final outputPath = '${tempDirectory.path}/$fileName';

        await _audioRecorder.start(
          RecordConfig(
            numChannels: 1,
            encoder: AudioEncoder.wav,
            autoGain: true,
            sampleRate: 16000,
            noiseSuppress: true,
          ),
          path: outputPath,
        );

        _setStateIfMounted(() {
          _isRecording = true;
          _secondsRemaining = 55;
          _recordingPath = outputPath;
        });

        _startTimer();
      } else {
        _requestMicrophonePermission();
      }
    } catch (e) {
      _showErrorDialog('Failed to start recording: $e');
    }
  }

  void _startTimer() {
    _timer = Timer.periodic(const Duration(seconds: 1), (timer) {
      if (!mounted) {
        timer.cancel();
        return;
      }

      setState(() {
        _secondsRemaining--;
      });

      if (_secondsRemaining <= 0) {
        _stopRecording();
      }
    });
  }

  Future<void> _stopRecording() async {
    try {
      _timer?.cancel();

      final String? path = await _audioRecorder.stop();

      if (!mounted) {
        return;
      }

      _setStateIfMounted(() {
        _isRecording = false;
        _secondsRemaining = 55;
        _recordingPath = path;
      });

      if (path != null) {
        // ask user if they want to upload
        _askUpload(path);
      }
    } catch (e) {
      _showErrorDialog('Failed to stop recording: $e');
    }
  }

  void _showErrorDialog(String message) {
    if (!mounted) {
      return;
    }

    showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Error'),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  Future<void> _showSuccessDialog(String message) async {
    if (!mounted) {
      return;
    }

    await showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Success'),
        content: Text(message),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  /// after recording finishes, prompt the user about uploading.
  Future<void> _askUpload(String path) async {
    if (!mounted) {
      return;
    }

    final shouldUpload = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Upload Recording?'),
        content: const Text('Are you satisfied with this recording and ready to upload it to your journal?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: const Text('No'),
          ),
          TextButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: const Text('Yes'),
          ),
        ],
      ),
    );

    if (!mounted) {
      return;
    }

    _setStateIfMounted(() {
      _recordingFinished = 'Recording was not saved! Start a new recording.';
    });

    if (shouldUpload == true) {
      await _uploadRecording(path);
    }
  }

  Future<void> _uploadRecording(String path) async {
    final model = context.read<OlieModel>();
    if (model.token == null) {
      _showErrorDialog('Not authenticated; please log in before uploading.');
      return;
    }

    _setStateIfMounted(() {
      _isUploading = true;
      _recordingFinished = 'Don\'t leave this page. Obtaining your location.';
    });

    Position? position;
    try {
      position = await _determinePosition();
    } catch (e) {
      // best-effort; ignore location failures but log
      debugPrint('Location error: $e');
    }

    _setStateIfMounted(() {
      _recordingFinished = 'Don\'t leave this page. Uploading now!';
    });

    try {
      final file = File(path);
      await Backend.uploadAudioEntry(
        model.token,
        file,
        latitude: position?.latitude,
        longitude: position?.longitude,
        onRetryScheduled: (retryInfo) {
          if (!mounted) {
            return;
          }
          _setStateIfMounted(() {
            _recordingFinished =
                'Weak reception detected. Waiting for cell reception '
                'and retrying upload (${retryInfo.nextAttempt}/${retryInfo.maxAttempts}) '
                'in ${retryInfo.retryDelay.inSeconds}s.';
          });
        },
      );
      _showSuccessDialog('Upload successful! Your submission is being processed, and a transcript along with chatbot feedback will be available shortly.');

      _setStateIfMounted(() {
        _isUploading = false;
        _recordingFinished = 'Recording uploaded!';
      });
    } catch (e) {
      final message = 'Unfortunately, your recording could not be saved. Please record a new entry to try again.\n\n$e';
      _setStateIfMounted(() {
        _isUploading = false;
        _recordingFinished = message;
      });
      _showErrorDialog(message);
    }
  }

  /// Returns current position or throws if location can't be determined.
  Future<Position> _determinePosition() async {
    bool serviceEnabled;
    LocationPermission permission;

    serviceEnabled = await Geolocator.isLocationServiceEnabled();
    if (!serviceEnabled) {
      throw Exception('Location services are disabled.');
    }

    permission = await Geolocator.checkPermission();
    if (permission == LocationPermission.denied) {
      permission = await Geolocator.requestPermission();
      if (permission == LocationPermission.denied) {
        throw Exception('Location permissions are denied');
      }
    }

    if (permission == LocationPermission.deniedForever) {
      throw Exception('Location permissions are permanently denied');
    }

    return await Geolocator.getCurrentPosition();
  }

  String _formatTime(int seconds) {
    final int minutes = seconds ~/ 60;
    final int secs = seconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${secs.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    final isLandscape =
        MediaQuery.orientationOf(context) == Orientation.landscape;
    final sectionGap = isLandscape ? 20.0 : 48.0;
    final instructionToCardGap = isLandscape ? 16.0 : 24.0;
    final timerCardPadding = isLandscape ? 20.0 : 32.0;

    return Scaffold(
      appBar: AppBar(title: const HomeHeader(), elevation: 0),
      body: Padding(
        padding: EdgeInsets.only(
          left: 16,
          right: 16,
          bottom: MediaQuery.viewPaddingOf(context).bottom,
        ),
        child: Column(
          children: [
            Expanded(
              child: SingleChildScrollView(
                child: Padding(
                  padding: const EdgeInsets.all(24.0),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      Text(
                        "Tap 'Start Recording' to begin. You have up to 1 minute. When you finish, tap 'Stop Recording' and choose 'Yes' to upload.",
                        textAlign: TextAlign.center,
                        style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                          fontWeight: FontWeight.w500,
                          color: Colors.black87,
                        ),
                      ),
                      SizedBox(height: instructionToCardGap),
                      Container(
                        padding: EdgeInsets.all(timerCardPadding),
                        decoration: BoxDecoration(
                          color: Colors.blue.shade50,
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(color: Colors.blue.shade300, width: 2),
                        ),
                        child: Column(
                          children: [
                            Text(
                              _isRecording ? 'Recording...' : 'Ready',
                              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: Colors.blue.shade600,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            const SizedBox(height: 16),
                            Text(
                              _formatTime(_secondsRemaining),
                              style: Theme.of(context).textTheme.displayLarge?.copyWith(
                                color: _isRecording ? Colors.red : Colors.blue,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                          ],
                        ),
                      ),
                      SizedBox(height: sectionGap),
                      if (_isRecording)
                        Row(
                          mainAxisAlignment: MainAxisAlignment.center,
                          children: [
                            Container(
                              width: 12,
                              height: 12,
                              decoration: BoxDecoration(
                                color: Colors.red,
                                borderRadius: BorderRadius.circular(6),
                              ),
                            ),
                            const SizedBox(width: 8),
                            const Text(
                              'Recording in progress...',
                              style: TextStyle(
                                color: Colors.red,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ],
                        )
                      else if (_isUploading)
                        Column(
                          children: [
                            const SizedBox(
                              width: 20,
                              height: 20,
                              child: CircularProgressIndicator(strokeWidth: 2),
                            ),
                            const SizedBox(height: 8),
                            Text(
                              _recordingFinished,
                              textAlign: TextAlign.center,
                              style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                                color: Colors.blue,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ],
                        )
                      else if (_recordingPath != null)
                        Text(
                          _recordingFinished,
                          textAlign: TextAlign.center,
                          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                            color: Colors.green,
                            fontWeight: FontWeight.w600,
                          ),
                        )
                      else
                        Text(
                          'No recording yet',
                          style: Theme.of(
                            context,
                          ).textTheme.bodyMedium?.copyWith(color: Colors.grey),
                        ),
                      SizedBox(height: sectionGap),
                      Wrap(
                        alignment: WrapAlignment.center,
                        spacing: 16,
                        runSpacing: 16,
                        children: [
                          ElevatedButton.icon(
                            onPressed: _isRecording || _isUploading ? null : _startRecording,
                            icon: const Icon(Icons.mic),
                            label: const Text('Start Recording'),
                            style: ElevatedButton.styleFrom(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 24,
                                vertical: 16,
                              ),
                              backgroundColor: Colors.blue,
                              disabledBackgroundColor: Colors.grey.shade400,
                            ),
                          ),
                          ElevatedButton.icon(
                            onPressed: _isRecording ? _stopRecording : null,
                            icon: const Icon(Icons.stop),
                            label: const Text('Stop Recording'),
                            style: ElevatedButton.styleFrom(
                              padding: const EdgeInsets.symmetric(
                                horizontal: 24,
                                vertical: 16,
                              ),
                              backgroundColor: Colors.red,
                              disabledBackgroundColor: Colors.grey.shade400,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
            ),
            const HomeFooter(),
          ],
        ),
      ),
    );
  }
}
