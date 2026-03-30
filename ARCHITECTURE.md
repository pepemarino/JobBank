# Works Commons – Architecture

## Core Concepts

### Interview System
- Dynamic question generation driven by the Interview Agent.
- Evaluation-based scoring using structured metrics only.
- No raw answer persistence; all ephemeral conversational data lives only in the browser.
- InterviewContext governs progression, topic coverage, and knowledge-gap detection.
- Browser-side state ensures continuity across refreshes and tab changes without exposing sensitive content to the server.

### Trainer System
- Consumes only the distilled, privacy-preserving AnalysisResult stored after an interview is completed.
- Uses hash-based caching to avoid recomputation.
- Generates personalized study plans from job descriptions and structured evaluation summaries.
- Never accesses raw interview content or user-provided text.

### LLM Integration
- API keys stored encrypted per user.
- Master encryption key sourced from environment variables.
- LLM prompts and responses used during interviews are never persisted to the database.
- All LLM interactions must comply with the privacy and data-minimization rules defined below.

### Privacy & Data Storage Requirements
User privacy, confidentiality, and data minimization are foundational principles of WORKS Commons.
The system is explicitly designed to prevent the storage of sensitive or personally revealing interview content.

### Prohibited Data (MUST NOT be stored in any system database)
The following categories of data are strictly forbidden from being persisted on the server, either directly or indirectly:

- Raw user answers (verbatim or paraphrased).
- Raw agent questions or LLM prompts.
- Full or partial conversation transcripts, including reconstructed or summarized versions.
- Personally identifiable information (PII) such as names, emails, phone numbers, or identifiers.
- Embarrassing, harmful, or reputation-sensitive content, even if voluntarily provided.
- Sensitive personal data, including emotional disclosures, health information, or protected characteristics.
- LLM internal reasoning, prompt history, or any content that could reconstruct the interview.

These prohibitions apply to:

- The Interview Agent
- The Training Agent
- Any derivative or extended project
- Any storage mechanism (database, logs, telemetry, analytics, backups)

### Allowed Data (Post-Interview Summary Only)
Only the following distilled, structured, privacy-preserving data may be stored after an interview concludes:

- CoveredTopics – high-level subject areas demonstrated.
- WeakAreas – conceptual gaps identified by the agent.
- Evaluations – structured evaluation objects:

-- Score
-- Passed (boolean)
-- Strengths (list of strings)
-- Gaps (list of strings)

- QuestionCount – optional; may be derived from evaluation count.

This data is intentionally abstracted to prevent reconstruction of the conversation or user behavior.

### Compliance Requirements
All agent-produced or agent-consumed data must comply with:

- OWASP Security Guidelines
- GDPR Privacy Principles, including:
-- Data minimization
-- Purpose limitation
-- Confidentiality
-- Right to erasure (browser-side ephemeral state supports this)

### Browser-Side Ephemeral State
During an active interview:

- All conversational history, partial evaluations, and agent internal state are stored only in the browser using encrypted local storage.
- This state is keyed by JobPostId to allow continuity across tabs and refreshes.
- This state is deleted immediately when the interview ends.
- This state is never transmitted to the server.

This ensures:

- No sensitive content leaves the user’s device.
- No server logs or backups contain interview text.
- The user retains full control over their ephemeral data.

## AGPL Compliance Clause
WORKS Commons is licensed under the GNU Affero General Public License (AGPL).
To protect user privacy and uphold the ethical intent of this project:

Any system, derivative work, or enhancement built on WORKS Commons that stores prohibited data (as defined above) is considered non‑compliant with this project’s privacy requirements and violates the terms under which WORKS Commons is distributed.

This clause ensures that:

- Privacy protections cannot be removed in forks.
- Derivative systems must honor the same confidentiality guarantees.
- Users are protected even when the software is modified or extended.

## Summary
WORKS Commons enforces a strict separation between:

- Ephemeral interview data (browser-only, private, never persisted)
- Structured evaluation summaries (server-side, privacy-preserving)

This architecture ensures:

- User confidentiality
- GDPR-aligned data minimization
- Ethical AI behavior
- Secure LLM integration
- A clean pipeline for the Training Agent