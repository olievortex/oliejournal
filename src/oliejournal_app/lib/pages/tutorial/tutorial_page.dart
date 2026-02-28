import 'package:flutter/material.dart';

class TutorialPage extends StatefulWidget {
  const TutorialPage({required this.onFinished, super.key});

  final Future<void> Function() onFinished;

  @override
  State<TutorialPage> createState() => _TutorialPageState();
}

class _TutorialPageState extends State<TutorialPage> {
  final PageController _pageController = PageController();

  static const List<_TutorialPaneData> _panes = [
    _TutorialPaneData(
      title: 'Learn from Every Chase',
      description:
          'Learn from the mistakes and decisions made in your prior storm chases.',
      icon: Icons.menu_book_rounded,
    ),
    _TutorialPaneData(
      title: 'Record Voice and Location',
      description:
          'Record your voice to generate a transcript automatically while capturing your current location.',
      icon: Icons.mic_rounded,
    ),
    _TutorialPaneData(
      title: 'Improve Entries with Feedback',
      description:
          'Use the chatbot feedback feature to improve your entries and build better storm journals over time.',
      icon: Icons.insights_rounded,
    ),
  ];

  int _currentPage = 0;
  bool _isFinishing = false;

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  Future<void> _finishTutorial() async {
    if (_isFinishing) {
      return;
    }

    setState(() {
      _isFinishing = true;
    });

    await widget.onFinished();
  }

  void _goToNextPage() {
    if (_currentPage >= _panes.length - 1) {
      _finishTutorial();
      return;
    }

    _pageController.nextPage(
      duration: const Duration(milliseconds: 250),
      curve: Curves.easeOut,
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            children: [
              Align(
                alignment: Alignment.centerRight,
                child: TextButton(
                  onPressed: _isFinishing ? null : _finishTutorial,
                  child: const Text('Skip'),
                ),
              ),
              Expanded(
                child: PageView.builder(
                  controller: _pageController,
                  onPageChanged: (index) {
                    setState(() {
                      _currentPage = index;
                    });
                  },
                  itemCount: _panes.length,
                  itemBuilder: (context, index) {
                    final pane = _panes[index];
                    return _TutorialPane(data: pane);
                  },
                ),
              ),
              const SizedBox(height: 20),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: List.generate(
                  _panes.length,
                  (index) => AnimatedContainer(
                    duration: const Duration(milliseconds: 200),
                    margin: const EdgeInsets.symmetric(horizontal: 4),
                    height: 8,
                    width: _currentPage == index ? 24 : 8,
                    decoration: BoxDecoration(
                      color: _currentPage == index
                          ? Theme.of(context).colorScheme.primary
                          : Theme.of(context).colorScheme.outlineVariant,
                      borderRadius: BorderRadius.circular(20),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 20),
              SizedBox(
                width: double.infinity,
                child: FilledButton(
                  onPressed: _isFinishing ? null : _goToNextPage,
                  child: Text(
                    _currentPage == _panes.length - 1 ? 'Get Started' : 'Next',
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _TutorialPane extends StatelessWidget {
  const _TutorialPane({required this.data});

  final _TutorialPaneData data;

  @override
  Widget build(BuildContext context) {
    final isLandscape =
        MediaQuery.orientationOf(context) == Orientation.landscape;
    final iconSize = isLandscape ? 72.0 : 96.0;
    final titleSpacing = isLandscape ? 20.0 : 32.0;
    final descriptionSpacing = isLandscape ? 12.0 : 16.0;

    return LayoutBuilder(
      builder: (context, constraints) {
        return SingleChildScrollView(
          child: ConstrainedBox(
            constraints: BoxConstraints(minHeight: constraints.maxHeight),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Icon(
                  data.icon,
                  size: iconSize,
                  color: Theme.of(context).colorScheme.primary,
                ),
                SizedBox(height: titleSpacing),
                Text(
                  data.title,
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.headlineSmall,
                ),
                SizedBox(height: descriptionSpacing),
                Text(
                  data.description,
                  textAlign: TextAlign.center,
                  style: Theme.of(context).textTheme.bodyLarge,
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}

class _TutorialPaneData {
  const _TutorialPaneData({
    required this.title,
    required this.description,
    required this.icon,
  });

  final String title;
  final String description;
  final IconData icon;
}