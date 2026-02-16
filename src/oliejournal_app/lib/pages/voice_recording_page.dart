import 'dart:async';
import 'package:flutter/material.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:path_provider/path_provider.dart';
import 'package:record/record.dart';

class VoiceRecordingPage extends StatefulWidget {
  const VoiceRecordingPage({super.key});

  @override
  State<VoiceRecordingPage> createState() => _VoiceRecordingPageState();
}

class _VoiceRecordingPageState extends State<VoiceRecordingPage> {
  final AudioRecorder _audioRecorder = AudioRecorder();
  Timer? _timer;
  int _secondsRemaining = 60;
  bool _isRecording = false;
  String? _recordingPath;

  @override
  void initState() {
    super.initState();
    _requestMicrophonePermission();
  }

  @override
  void dispose() {
    _timer?.cancel();
    _audioRecorder.dispose();
    super.dispose();
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

        setState(() {
          _isRecording = true;
          _secondsRemaining = 60;
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

      setState(() {
        _isRecording = false;
        _secondsRemaining = 60;
        _recordingPath = path;
      });

      if (path != null) {
        _showSuccessDialog('Recording saved to:\n$path');
      }
    } catch (e) {
      _showErrorDialog('Failed to stop recording: $e');
    }
  }

  void _showErrorDialog(String message) {
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

  void _showSuccessDialog(String message) {
    showDialog(
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

  String _formatTime(int seconds) {
    final int minutes = seconds ~/ 60;
    final int secs = seconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${secs.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    // wrap main content in an Expanded so footer stays at bottom
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
              child: Center(
                child: Padding(
                  padding: const EdgeInsets.all(24.0),
                  child: Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      // Instructions plus timer display
                      Text(
                        'Tap "Start Recording" and begin dictating your field notes. You have one minute per entry.',
                        textAlign: TextAlign.center,
                        style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                          fontWeight: FontWeight.w500,
                          color: Colors.black87,
                        ),
                      ),
                      const SizedBox(height: 24),
                      Container(
                        padding: const EdgeInsets.all(32),
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
                      const SizedBox(height: 48),

                      // Recording Status
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
                      else if (_recordingPath != null)
                        Text(
                          'Last recording saved',
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
                      const SizedBox(height: 48),

                      // Buttons – use Wrap to avoid horizontal overflow on narrow screens
                      Wrap(
                        alignment: WrapAlignment.center,
                        spacing: 16,
                        runSpacing: 16,
                        children: [
                          // Start Recording Button
                          ElevatedButton.icon(
                            onPressed: _isRecording ? null : _startRecording,
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

                          // Stop Recording Button
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
