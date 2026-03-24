# Works Commons - Architecture

## Core Concepts

### Interview System
- Dynamic question generation
- Evaluation-based scoring (no raw answer persistence)
- InterviewContext drives progression

### Trainer System
- Uses cached AnalysisResult (hash-based)
- Generates study plan from job description

### LLM Integration
- API keys stored encrypted per user
- Master key from environment