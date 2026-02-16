import 'dart:async';

import 'package:flutter/material.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:provider/provider.dart';
import 'package:audioplayers/audioplayers.dart';

/// Displays everything the backend sends back for a single entry.
///
/// The list page pushes this route when the user taps a tile.  The UI
/// includes the transcript, response text and, if available, a simple
/// play/stop control for the response audio.
class JournalEntryDetailPage extends StatefulWidget {
  const JournalEntryDetailPage({super.key, required this.entry});

  final JournalEntryModel entry;

  @override
  State<JournalEntryDetailPage> createState() => _JournalEntryDetailPageState();
}

class _JournalEntryDetailPageState extends State<JournalEntryDetailPage> {
  final AudioPlayer _audioPlayer = AudioPlayer();
  bool _playing = false;

  Timer? _reloadTimer;

  @override
  void initState() {
    super.initState();
    _audioPlayer.onPlayerComplete.listen((_) {
      setState(() {
        _playing = false;
      });
    });
  }

  @override
  void dispose() {
    _reloadTimer?.cancel();
    _audioPlayer.dispose();
    super.dispose();
  }

  Future<void> _play() async {
    final url = widget.entry.responsePath;
    if (url == null) return;

    final messenger = ScaffoldMessenger.of(context);
    try {
      await _audioPlayer.setSource(UrlSource(url));
      await _audioPlayer.resume();
      setState(() {
        _playing = true;
      });
    } catch (e) {
      messenger.showSnackBar(
        const SnackBar(content: Text('Could not play audio')),
      );
    }
  }

  Future<void> _stop() async {
    await _audioPlayer.stop();
    setState(() {
      _playing = false;
    });
  }

  void _maybeStartTimer(JournalEntryModel entry, OlieModel model) {
    if (entry.responsePath == null && _reloadTimer == null) {
      _reloadTimer = Timer(const Duration(seconds: 20), () async {
        await model.fetchEntryStatus(entry.id);
        setState(() {
          _reloadTimer?.cancel();
          _reloadTimer = null;
        });
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final model = context.watch<OlieModel>();
    JournalEntryModel entry;
    try {
      entry = model.journalEntries.firstWhere((e) => e.id == widget.entry.id);
    } catch (_) {
      entry = widget.entry;
    }

    // if the entry still doesn't have a response path we'll schedule
    // another refresh.  cancel timer if we already got one.
    if (entry.responsePath != null) {
      _reloadTimer?.cancel();
      _reloadTimer = null;
    } else {
      _maybeStartTimer(entry, model);
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('Entry Details'),
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Reload entry',
            onPressed: () => model.fetchEntryStatus(entry.id),
          ),
        ],
      ),
      body: Padding(
        padding: EdgeInsets.only(
          left: 16,
          right: 16,
          top: 16,
          bottom: MediaQuery.viewPaddingOf(context).bottom,
        ),
        child: Column(
          children: [
            Expanded(
              child: ListView(
                children: [
                  Text('ID: ${entry.id}'),
                  const SizedBox(height: 8),
                  Text('Created: ${entry.created.toLocal()}'),
                  const SizedBox(height: 16),
                  const Text('Transcript:', style: TextStyle(fontWeight: FontWeight.bold)),
                  const SizedBox(height: 4),
                  Text(entry.transcript ?? '—'),
                  const SizedBox(height: 16),
                  const Text('Response text:', style: TextStyle(fontWeight: FontWeight.bold)),
                  const SizedBox(height: 4),
                  Text(entry.responseText ?? '—'),
                  const SizedBox(height: 16),
                  if (entry.responsePath != null) ...[
                    const Text('Response audio:', style: TextStyle(fontWeight: FontWeight.bold)),
                    Row(
                      children: [
                        IconButton(
                          icon: const Icon(Icons.play_arrow),
                          onPressed: _playing ? null : _play,
                        ),
                        IconButton(
                          icon: const Icon(Icons.stop),
                          onPressed: _playing ? _stop : null,
                        ),
                      ],
                    ),
                  ],
                ],
              ),
            ),
            const HomeFooter(),
          ],
        ),
      ),
    );
  }
}
