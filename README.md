# OlieJournal

**The storm chasing observation tool that turns your voice into documented decisions, observations, and waypoints—with AI feedback to sharpen your chase skills.**

OlieJournal is designed for real-time use during storm chases. Speak your observations, log critical decisions, and mark locations—all while keeping your eyes on the storm.

## What OlieJournal Does

### Voice-First Capture
Dictate observations hands-free while driving. OlieJournal transcribes your voice into structured entries instantly—whether you're noting a developing wall cloud, logging a directional decision, or marking a promising intercept location.

### AI-Powered Feedback
Get intelligent insights on your observations to improve your chase decision-making and weather analysis skills. The built-in chatbot analyzes your notes and provides constructive feedback to help you become a more effective storm chaser.

### Location Tracking
Automatically capture GPS coordinates with each entry. Mark key locations and visualize your chase path on a map. Every entry includes GPS coordinates mapped out with full location context on an interactive map.

### Chase Decisions Logger
Document why you chose specific routes or targets. Build a comprehensive record of decision-making with timestamps, locations, and detailed notes for post-chase analysis and sharing.

### Weather Observation Notes
Capture real-time conditions, cloud structures, and atmospheric changes while they're happening in the field. Speak naturally—your observations are transcribed accurately so you can focus on the chase.

### Mobile-Optimized Design
Built with Flutter for fast, reliable performance in the field—even during data-intensive chase days. Optimized for field conditions with fast transcription, offline capability, and instant GPS logging.

---

## Technical Overview

I built OlieJournal as a storm-chasing observation platform that combines a Flutter app with an ASP.NET Core backend and an asynchronous processing pipeline to transcribe audio, generate AI feedback, and create voice responses.

I developed most of the Flutter application with GitHub Copilot, showcasing my ability to ship effectively with AI-assisted tools.

### System Architecture
I designed OlieJournal as a multi-tier system built around asynchronous processing:

- **Client**: Flutter app records audio and captures location metadata.
- **API**: ASP.NET Core API accepts uploads and orchestrates workflow.
- **Queue workers**: CLI services process long-running tasks from Azure Service Bus.
- **Processing pipeline**:
	- Google Cloud Speech-to-Text transcribes recordings.
	- OpenAI analyzes transcripts and produces coaching feedback.
	- Google Text-to-Speech generates spoken responses.
- **Storage**:
	- MySQL stores journal entries, conversations, and metadata.
	- Azure Blob Storage stores source audio, converted audio, and backups.

See [architecture.txt](architecture.txt) for the architecture diagram.

### How It Works
1. **Capture and upload (Client → API)**
	- Flutter records audio and attaches location metadata.
	- Client uploads payload to the ASP.NET Core API.

2. **Persist and enqueue (API)**
	- API stores raw audio in Azure Blob Storage.
	- API writes initial journal metadata to MySQL.
	- API publishes a processing message to Azure Service Bus.

3. **Transcription stage (Worker)**
	- Background CLI worker consumes the queue message.
	- Worker retrieves blob audio and calls Google Speech-to-Text.
	- Transcript and processing status are persisted to MySQL.

4. **Analysis stage (Worker)**
	- Chatbot worker receives transcript-ready work.
	- Worker sends transcript + prompt context to OpenAI.
	- AI response is saved to conversation/journal tables.

5. **Voice synthesis stage (Worker)**
	- Voiceover worker sends chatbot output to Google Text-to-Speech.
	- Generated audio is stored in Blob Storage.
	- Final artifact references and completion state are written to MySQL.

6. **Operational behavior**
	- Queue-backed workers decouple API latency from long-running jobs.
	- Each stage can be scaled independently based on queue depth.
	- Failures are isolated per stage and can be retried via message reprocessing.

## Setup
1. Complete infrastructure setup: [infrastructure/README.md](infrastructure/README.md)
2. Complete project installation: [install/README.md](install/README.md)

---

**Learn more at [oliejournal.olievortex.com](https://oliejournal.olievortex.com)**