import 'package:flutter/material.dart';
import 'package:oliejournal_app/models/olie_model.dart';
import 'package:oliejournal_app/pages/home/home_page.dart';
import 'package:provider/provider.dart';

class DeleteAllDataPage extends StatefulWidget {
  const DeleteAllDataPage({super.key});

  @override
  State<DeleteAllDataPage> createState() => _DeleteAllDataPageState();
}

class _DeleteAllDataPageState extends State<DeleteAllDataPage> {
  final TextEditingController _confirmationController = TextEditingController();
  bool _isDeleting = false;

  bool get _canDelete => _confirmationController.text.trim() == 'DELETE';

  @override
  void dispose() {
    _confirmationController.dispose();
    super.dispose();
  }

  Future<void> _onDeletePressed() async {
    if (_isDeleting || !_canDelete) {
      return;
    }

    setState(() {
      _isDeleting = true;
    });

    final olieModel = context.read<OlieModel>();

    try {
      await olieModel.deleteAllUserData();

      if (!mounted) {
        return;
      }

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Your data has been deleted.')),
      );
      Navigator.of(context).pushAndRemoveUntil(
        MaterialPageRoute(builder: (context) => HomePage()),
        (Route<dynamic> route) => false,
      );
    } catch (_) {
      if (!mounted) {
        return;
      }

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Could not delete your data right now.')),
      );
      setState(() {
        _isDeleting = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Delete all data')),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: SingleChildScrollView(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const Text(
                  'This action permanently deletes all your OlieJournal information and cannot be undone.',
                ),
                const SizedBox(height: 16),
                const Text('Type DELETE to confirm.'),
                const SizedBox(height: 8),
                TextField(
                  controller: _confirmationController,
                  autofocus: true,
                  textInputAction: TextInputAction.done,
                  onChanged: (_) {
                    setState(() {});
                  },
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    hintText: 'DELETE',
                  ),
                ),
                const SizedBox(height: 20),
                SizedBox(
                  width: double.infinity,
                  child: FilledButton(
                    onPressed: _isDeleting || !_canDelete ? null : _onDeletePressed,
                    child: _isDeleting
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(strokeWidth: 2),
                          )
                        : const Text('Delete all data'),
                  ),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
