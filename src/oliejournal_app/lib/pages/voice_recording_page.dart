import 'package:flutter/material.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';
import 'package:oliejournal_app/pages/voice_recording/voice_recording_controller.dart';
import 'package:provider/provider.dart';

class VoiceRecordingPage extends StatefulWidget {
  const VoiceRecordingPage({super.key});

  @override
  State<VoiceRecordingPage> createState() => _VoiceRecordingPageState();
}

class _VoiceRecordingPageState extends State<VoiceRecordingPage> {
  final VoiceRecordingController _controller = VoiceRecordingController();

  @override
  void initState() {
    super.initState();
    _controller.onAutoStop = _onAutoStop;
    _fetchPermissions();
  }

  @override
  void dispose() {
    _controller.onAutoStop = null;
    _controller.dispose();
    super.dispose();
  }

  Future<void> _onAutoStop(String path) async {
    if (!mounted) {
      return;
    }

    await _askUpload(path);
  }

  Future<void> _fetchPermissions() async {
    final warnings = await _controller.requestPermissions();
    if (!mounted || warnings.isEmpty) {
      return;
    }

    final messenger = ScaffoldMessenger.of(context);
    messenger.showSnackBar(SnackBar(content: Text(warnings.join('\n'))));
  }

  Future<void> _startRecording() async {
    try {
      await _controller.startRecording();
    } catch (e) {
      _showErrorDialog('Failed to start recording: $e');
    }
  }

  Future<void> _stopRecording() async {
    try {
      final path = await _controller.stopRecording();

      if (path != null) {
        await _askUpload(path);
      }
    } catch (e) {
      _showErrorDialog('Failed to stop recording: $e');
    }
  }

  void _showErrorDialog(String message) {
    _showMessageDialog(title: 'Error', message: message);
  }

  Future<void> _showSuccessDialog(String message) async {
    await _showMessageDialog(title: 'Success', message: message);
  }

  Future<void> _showMessageDialog({
    required String title,
    required String message,
  }) async {
    if (!mounted) {
      return;
    }

    await showDialog(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(title),
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

    _controller.markRecordingNotSaved();

    if (shouldUpload == true) {
      await _uploadRecording(path);
    }
  }

  Future<void> _uploadRecording(String path) async {
    final model = context.read<OlieModel>();

    try {
      await _controller.uploadRecording(
        path: path,
        token: model.token,
      );
      _showSuccessDialog('Upload successful! Your submission is being processed, and a transcript along with chatbot feedback will be available shortly.');
    } catch (e) {
      _showErrorDialog('$e');
    }
  }

  String _formatTime(int seconds) {
    final int minutes = seconds ~/ 60;
    final int secs = seconds % 60;
    return '${minutes.toString().padLeft(2, '0')}:${secs.toString().padLeft(2, '0')}';
  }

  ButtonStyle _recordingButtonStyle(Color backgroundColor) {
    return ElevatedButton.styleFrom(
      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 16),
      backgroundColor: backgroundColor,
      disabledBackgroundColor: Colors.grey.shade400,
    );
  }

  Widget _buildInstructions(BuildContext context) {
    return Text(
      "Tap 'Start Recording' to begin. You have up to 1 minute. When you finish, tap 'Stop Recording' and choose 'Yes' to upload.",
      textAlign: TextAlign.center,
      style: Theme.of(
        context,
      ).textTheme.bodyLarge?.copyWith(fontWeight: FontWeight.w500, color: Colors.black87),
    );
  }

  Widget _buildTimerCard(BuildContext context, double timerCardPadding) {
    return Container(
      padding: EdgeInsets.all(timerCardPadding),
      decoration: BoxDecoration(
        color: Colors.blue.shade50,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: Colors.blue.shade300, width: 2),
      ),
      child: Column(
        children: [
          Text(
            _controller.isRecording ? 'Recording...' : 'Ready',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              color: Colors.blue.shade600,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 16),
          Text(
            _formatTime(_controller.secondsRemaining),
            style: Theme.of(context).textTheme.displayLarge?.copyWith(
              color: _controller.isRecording ? Colors.red : Colors.blue,
              fontWeight: FontWeight.bold,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatusContent(BuildContext context) {
    if (_controller.isRecording) {
      return Row(
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
            style: TextStyle(color: Colors.red, fontWeight: FontWeight.w600),
          ),
        ],
      );
    }

    if (_controller.isUploading) {
      return Column(
        children: [
          const SizedBox(
            width: 20,
            height: 20,
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
          const SizedBox(height: 8),
          Text(
            _controller.recordingFinished,
            textAlign: TextAlign.center,
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
              color: Colors.blue,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      );
    }

    if (_controller.recordingPath != null) {
      return Text(
        _controller.recordingFinished,
        textAlign: TextAlign.center,
        style: Theme.of(
          context,
        ).textTheme.bodyMedium?.copyWith(color: Colors.green, fontWeight: FontWeight.w600),
      );
    }

    return Text(
      'No recording yet',
      style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: Colors.grey),
    );
  }

  Widget _buildActionButtons() {
    return Wrap(
      alignment: WrapAlignment.center,
      spacing: 16,
      runSpacing: 16,
      children: [
        ElevatedButton.icon(
          onPressed: _controller.isRecording || _controller.isUploading
              ? null
              : _startRecording,
          icon: const Icon(Icons.mic),
          label: const Text('Start Recording'),
          style: _recordingButtonStyle(Colors.blue),
        ),
        ElevatedButton.icon(
          onPressed: _controller.isRecording ? _stopRecording : null,
          icon: const Icon(Icons.stop),
          label: const Text('Stop Recording'),
          style: _recordingButtonStyle(Colors.red),
        ),
      ],
    );
  }

  Widget _buildRecordingContent(
    BuildContext context, {
    required double sectionGap,
    required double instructionToCardGap,
    required double timerCardPadding,
  }) {
    return Padding(
      padding: const EdgeInsets.all(24.0),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          _buildInstructions(context),
          SizedBox(height: instructionToCardGap),
          _buildTimerCard(context, timerCardPadding),
          SizedBox(height: sectionGap),
          _buildStatusContent(context),
          SizedBox(height: sectionGap),
          _buildActionButtons(),
        ],
      ),
    );
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
              child: AnimatedBuilder(
                animation: _controller,
                builder: (context, child) {
                  return SingleChildScrollView(
                    child: _buildRecordingContent(
                      context,
                      sectionGap: sectionGap,
                      instructionToCardGap: instructionToCardGap,
                      timerCardPadding: timerCardPadding,
                    ),
                  );
                },
              ),
            ),
            const HomeFooter(),
          ],
        ),
      ),
    );
  }
}
