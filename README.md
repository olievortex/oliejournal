# OlieJournal

I built OlieJournal as a storm-chasing observation platform that helps chasers in the field capture voice notes, geolocation, and decision context in real time.

I combined a Flutter app with an ASP.NET Core backend and an asynchronous processing pipeline to transcribe audio, generate AI feedback, and create voice responses.

I developed most of the Flutter application with GitHub Copilot, showcasing my ability to ship effectively with AI-assisted tools.

## What OlieJournal Does
- **Hands-free journaling**: Capture observations and decisions while staying focused on the environment.
- **Location-aware entries**: Geocode posts so sessions can be reviewed by route and location.
- **AI-assisted improvement**: Receive chatbot prompts and feedback to improve observation quality over time.
- **Cross-platform client**: Flutter-based app targets mobile and web experiences.

## System Architecture
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

## How It Works
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
