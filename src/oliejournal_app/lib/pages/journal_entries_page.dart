import 'dart:async';

import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:oliejournal_app/constants.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';
import 'package:oliejournal_app/pages/journal_entry_detail_page.dart';
import 'package:provider/provider.dart';
import 'package:audioplayers/audioplayers.dart';

/// A simple screen that displays the journal entries returned from the backend.
///
/// Entries that already have a `responsePath` will show play/stop buttons
/// which use an in‑app audio player (`audioplayers`).  Entries without a
/// response path will have a 20‑second timer started; when the timer fires the
/// entire list is refreshed and the timer for that entry is cancelled if the
/// response path is no longer null.
class JournalEntriesPage extends StatefulWidget {
  const JournalEntriesPage({super.key});

  @override
  State<JournalEntriesPage> createState() => _JournalEntriesPageState();
}

class _JournalEntriesPageState extends State<JournalEntriesPage> {
  /// timers keyed by entry id so that we don't re-create the same timer every
  /// time build() runs.
  final Map<int, Timer> _reloadTimers = {};

  /// single shared audio player used by the page; we don't need multiple
  /// instances because user can only listen to one entry at a time.
  final AudioPlayer _audioPlayer = AudioPlayer();

  /// keep track of what's currently playing so we can optionally update the
  /// UI if desired.  we store the entry id or `null` when stopped.
  int? _playingEntryId;

  @override
  void initState() {
    super.initState();
    // when playback completes we should clear the playing entry so the UI
    // can update accordingly.
    _audioPlayer.onPlayerComplete.listen((_) {
      setState(() {
        _playingEntryId = null;
      });
    });
  }

  @override
  void dispose() {
    for (final timer in _reloadTimers.values) {
      timer.cancel();
    }
    _reloadTimers.clear();
    _audioPlayer.dispose();
    super.dispose();
  }

  void _maybeStartTimerForEntry(JournalEntryModel entry, OlieModel model) {
    if (entry.responsePath == null && !_reloadTimers.containsKey(entry.id)) {
      _reloadTimers[entry.id] = Timer(const Duration(seconds: 20), () async {
        // now that we have a dedicated endpoint we only refresh this entry.
        final updated = await model.fetchEntryStatus(entry.id);
        setState(() {
          // Our timer is a one-shot, so delete it.
          _reloadTimers.remove(entry.id)?.cancel();
        });

        // Try it again
        _maybeStartTimerForEntry(updated ?? entry, model);
      });
    }
  }

  /// start playing an audio file from the given URL inside the app using
  /// the `audioplayers` package.  we also update [_playingEntryId] to keep
  /// track of the currently active entry.
  Future<void> _playUrl(
    String url,
    int entryId,
    ScaffoldMessengerState messenger,
  ) async {
    try {
      await _audioPlayer.setSource(UrlSource(url));
      await _audioPlayer.resume();
      setState(() {
        _playingEntryId = entryId;
      });
    } catch (e) {
      messenger.showSnackBar(
        const SnackBar(content: Text('Could not play audio')),
      );
    }
  }

  /// stop whatever is currently playing and clear the tracking id.
  Future<void> _stopPlayback() async {
    await _audioPlayer.stop();
    setState(() {
      _playingEntryId = null;
    });
  }

  @override
  Widget build(BuildContext context) {
    final messenger = ScaffoldMessenger.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const HomeHeader(),
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Reload entries',
            onPressed: () {
              // simply ask the model to reload; the loading indicator will
              // show automatically via Consumer above.
              context.read<OlieModel>().fetchEntries();
            },
          ),
        ],
      ),
      body: Padding(
        padding: EdgeInsets.only(
          left: 16,
          right: 16,
          bottom: MediaQuery.viewPaddingOf(context).bottom,
        ),
        child: Column(
          children: [
            const Text('Journal Entries'),
            const SizedBox(height: 16),
            Expanded(child: _entryList(messenger)),
            const HomeFooter(),
          ],
        ),
      ),
    );
  }

  Widget _entryList(ScaffoldMessengerState messenger) {
    return Consumer<OlieModel>(
      builder: (context, model, child) {
        if (model.isLoading) {
          return const Center(child: CircularProgressIndicator.adaptive());
        }

        if (model.journalEntries.isEmpty) {
          return Center(
            child: Text(model.errorMessage ?? 'No entries available'),
          );
        }

        // kick off timers for entries that need them.  we do this inside the
        // builder so that whenever the list changes we can evaluate again.
        for (final entry in model.journalEntries) {
          _maybeStartTimerForEntry(entry, model);
        }

        return ListView.builder(
          itemCount: model.journalEntries.length,
          itemBuilder: (context, index) {
            final entry = model.journalEntries[index];
            return _entryTile(entry, messenger);
          },
        );
      },
    );
  }

  String _limitTextWordSafe(String text, int maxLength) {
    if (text.length <= maxLength) return text;

    // Truncate and find the last space to avoid cutting a word
    String subString = text.substring(0, maxLength);
    var result = subString.substring(0, subString.lastIndexOf(' ')).trim();

    result = '$result...';

    return result;
  }

  String _formatLocalTime(DateTime value) {
    var format = DateFormat.yMd().add_jm();
    return format.format(value.toLocal());
  }

  Widget _entryTile(JournalEntryModel entry, ScaffoldMessengerState messenger) {
    final String fullText = entry.transcript ?? 'Waiting for transcript...';
    final snippet = _limitTextWordSafe(fullText, 200);

    return Card(
      margin: const EdgeInsets.symmetric(vertical: 8),
      child: InkWell(
        onTap: () {
          Navigator.of(context).push(
            MaterialPageRoute(
              builder: (_) => JournalEntryDetailPage(entry: entry),
            ),
          );
        },
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(snippet, style: kRobotoText),
              Row(
                children: [
                  Text(
                    'Created: ${_formatLocalTime(entry.created)}',
                    style: kRobotoText.copyWith(
                      fontSize: kBodySmall,
                      color: kColorGrey,
                    ),
                  ),
                  const SizedBox(width: 16),
                ],
              ),
              Align(
                alignment: Alignment.centerRight,
                child: entry.responsePath != null
                    ? Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          Text(
                            'Chatbot response:',
                            style: kRobotoText.copyWith(
                              fontSize: kBodySmall,
                              color: kColorGrey,
                            ),
                          ),
                          IconButton(
                            icon: const Icon(Icons.play_arrow),
                            onPressed: () => _playUrl(
                              entry.responsePath!,
                              entry.id,
                              messenger,
                            ),
                          ),
                          IconButton(
                            icon: const Icon(Icons.stop),
                            onPressed: _playingEntryId == entry.id
                                ? _stopPlayback
                                : null,
                          ),
                        ],
                      )
                    : const SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
