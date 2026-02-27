import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:latlong2/latlong.dart';
import 'package:oliejournal_app/backend.dart';
import 'package:oliejournal_app/models/journal_entry_model.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/components/home_footer.dart';
import 'package:provider/provider.dart';
import 'package:audioplayers/audioplayers.dart';
import 'package:intl/intl.dart';

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
  final MapController _mapController = MapController();
  final format = DateFormat.yMd().add_jm();
  bool _playing = false;
  bool _isDeleting = false;

  Timer? _reloadTimer;

  @override
  void initState() {
    super.initState();
    _audioPlayer.onPlayerComplete.listen((_) {
      if (!mounted) return;
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

  Future<void> _play(String? url) async {
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
        if (!mounted) return;

        final updated = await model.fetchEntryStatus(entry.id);
        if (!mounted) return;

        _reloadTimer?.cancel();
        _reloadTimer = null;

        _maybeStartTimer(updated ?? entry, model);
      });
    }
  }

  Future<void> _confirmAndDelete(JournalEntryModel entry, OlieModel model) async {
    final shouldDelete = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Delete entry?'),
          content: const Text(
            'Are you sure you want to delete this journal entry? This action cannot be undone.',
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(false),
              child: const Text('Cancel'),
            ),
            FilledButton(
              onPressed: () => Navigator.of(context).pop(true),
              child: const Text('Delete'),
            ),
          ],
        );
      },
    );

    if (shouldDelete != true || !mounted) {
      return;
    }

    setState(() {
      _isDeleting = true;
    });

    final messenger = ScaffoldMessenger.of(context);
    try {
      await Backend.deleteJournalEntry(entry.id, model.token);
      await model.fetchEntries();

      if (!mounted) return;
      Navigator.of(context).pop();
    } catch (_) {
      if (!mounted) return;
      messenger.showSnackBar(
        const SnackBar(content: Text('Could not delete entry')),
      );
    } finally {
      if (mounted) {
        setState(() {
          _isDeleting = false;
        });
      }
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
          IconButton(
            icon: _isDeleting
                ? const SizedBox(
                    width: 20,
                    height: 20,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.delete_outline),
            tooltip: 'Delete entry',
            onPressed: _isDeleting
                ? null
                : () async {
                    await _confirmAndDelete(entry, model);
                  },
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
                  Text('Created: ${format.format(entry.created.toLocal())}'),
                  const SizedBox(height: 16),
                  const Text(
                    'Transcript:',
                    style: TextStyle(fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 4),
                  Text(entry.transcript ?? 'Processing...'),
                  const SizedBox(height: 16),
                  const Text(
                    'Chatbot response:',
                    style: TextStyle(fontWeight: FontWeight.bold),
                  ),
                  const SizedBox(height: 4),
                  Text(entry.responseText ?? 'Processing...'),
                  const SizedBox(height: 16),
                  if (entry.latitude != null && entry.longitude != null) ...[
                    const Text(
                      'Location:',
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                    const SizedBox(height: 4),
                    SizedBox(
                      height: 220,
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(12),
                        child: Stack(
                          children: [
                            FlutterMap(
                              mapController: _mapController,
                              options: MapOptions(
                                initialCenter: LatLng(
                                  entry.latitude!,
                                  entry.longitude!,
                                ),
                                initialZoom: 14,
                                interactionOptions: const InteractionOptions(
                                  flags: InteractiveFlag.pinchZoom |
                                      InteractiveFlag.drag,
                                ),
                              ),
                              children: [
                                TileLayer(
                                  urlTemplate:
                                      'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                                  userAgentPackageName: 'oliejournal_app',
                                ),
                                MarkerLayer(
                                  markers: [
                                    Marker(
                                      point: LatLng(
                                        entry.latitude!,
                                        entry.longitude!,
                                      ),
                                      width: 40,
                                      height: 40,
                                      child: const Icon(
                                        Icons.location_pin,
                                        color: Colors.red,
                                        size: 36,
                                      ),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                            Positioned(
                              right: 8,
                              bottom: 8,
                              child: Column(
                                children: [
                                  FloatingActionButton.small(
                                    heroTag: 'zoom_in',
                                    onPressed: () {
                                      final currentZoom = _mapController.camera.zoom;
                                      _mapController.move(
                                        _mapController.camera.center,
                                        currentZoom + 1,
                                      );
                                    },
                                    child: const Icon(Icons.add),
                                  ),
                                  const SizedBox(height: 8),
                                  FloatingActionButton.small(
                                    heroTag: 'zoom_out',
                                    onPressed: () {
                                      final currentZoom = _mapController.camera.zoom;
                                      _mapController.move(
                                        _mapController.camera.center,
                                        currentZoom - 1,
                                      );
                                    },
                                    child: const Icon(Icons.remove),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),
                  ],
                  if (entry.responsePath != null) ...[
                    const Text(
                      'Chatbot audio:',
                      style: TextStyle(fontWeight: FontWeight.bold),
                    ),
                    Row(
                      children: [
                        IconButton(
                          icon: const Icon(Icons.play_arrow),
                          onPressed: _playing
                              ? null
                              : () async {
                                  _play(entry.responsePath);
                                },
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
