import 'dart:async';

import 'package:flutter/material.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:oliejournal_app/pages/home/components/home_header.dart';
import 'package:provider/provider.dart';
import 'package:url_launcher/url_launcher.dart';

/// A simple screen that displays the journal entries returned from the backend.
///
/// Entries that already have a `responsePath` will show a play button which
/// attempts to play the audio using `url_launcher`.  Entries without a response
/// path will have a 20‑second timer started; when the timer fires the entire
/// list is refreshed and the timer for that entry is cancelled if the
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

  @override
  void dispose() {
    for (final timer in _reloadTimers.values) {
      timer.cancel();
    }
    _reloadTimers.clear();
    super.dispose();
  }

  void _maybeStartTimerForEntry(JournalEntryModel entry, OlieModel model) {
    if (entry.responsePath == null && !_reloadTimers.containsKey(entry.id)) {
      _reloadTimers[entry.id] = Timer(const Duration(seconds: 20), () async {
        // now that we have a dedicated endpoint we only refresh this entry.
        await model.fetchEntryStatus(entry.id);
        setState(() {
          // always remove our timer after it fires. if the entry still lacks a
          // response path we'll recreate a new timer in the next build pass.
          _reloadTimers.remove(entry.id)?.cancel();
        });
      });
    }
  }

  /// helper to launch the audio URL.  we rely on `url_launcher` to choose the
  /// appropriate handler for the mime-type; the backend is expected to
  /// provide an http(s) link to a playable audio file.
  Future<void> _playUrl(String url) async {
    final uri = Uri.tryParse(url);
    if (uri == null) return;
    if (!await launchUrl(uri)) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not play audio')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
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
            const Text('Journal Entries'),
            const SizedBox(height: 16),
            Expanded(child: _entryList()),
            const HomeFooter(),
          ],
        ),
      ),
    );
  }

  Widget _entryList() {
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
            return _entryTile(entry);
          },
        );
      },
    );
  }

  Widget _entryTile(JournalEntryModel entry) {
    final String fullText = entry.transcript ?? '';
    final snippet = fullText.length <= 100 ? fullText : fullText.substring(0, 100);

    return Card(
      margin: const EdgeInsets.symmetric(vertical: 8),
      child: ListTile(
        title: Text(snippet),
        subtitle: Text('Created: ${entry.created.toLocal()}'),
        trailing: entry.responsePath != null
            ? IconButton(
                icon: const Icon(Icons.play_arrow),
                onPressed: () => _playUrl(entry.responsePath!),
              )
            : const SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
      ),
    );
  }
}
