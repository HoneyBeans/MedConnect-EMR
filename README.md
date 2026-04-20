# MedConnect: Enterprise EMR System

A full-stack Electronic Medical Record (EMR) platform inspired by the workflow of the UMS Poliklinik. This project replaces manual, fragmented clinic procedures with a systematic, role-based pipeline to efficiently manage patient queues and centralize medical histories.

## System Architecture

MedConnect operates as a Finite State Machine (FSM). Rather than a simple data-entry form, it routes patients through a centralized, sequential pipeline. By providing dedicated dashboard views for specific medical roles, it creates a seamless data flow from the moment a patient registers to the moment they receive their medication.

### Core Features
* **Role-Based Clinical Pipeline:** Distinct dashboard interfaces for Triage Nurses, Attending Physicians, and Pharmacists.
* **State-Driven Queues:** Patients automatically move between waiting lists (e.g., "Waiting for Doctor" to "Waiting for Pharmacy") upon stage completion.
* **Centralized Patient Database:** Comprehensive medical profiles, including vital histories, tracked allergies, and complete clinical encounter logs.
* **Single Page Application (SPA):** A dynamic, asynchronous frontend that updates without page reloads using the Fetch API and Tailwind CSS.

## Tech Stack
* **Backend:** C# / .NET Web API
* **Database:** Microsoft SQL Server (Relational architecture with custom Stored Procedures)
* **Frontend:** HTML5, Vanilla JavaScript, Tailwind CSS

## Future Enhancements & Roadmap
This project serves as a foundational architecture, with the following enterprise features planned for integration:
* **IoT Hardware Integration:** Integrating ESP32 microcontrollers with digital blood pressure and temperature sensors to automatically populate the Nurse Triage queue via JSON payloads.
* **Identity & Security Layer:** Implementing JSON Web Tokens (JWT) for strict Role-Based Access Control (RBAC) and exploring OAuth2 integration for potential MySejahtera synchronization.
* **Automated Webhook Notifications:** Triggering SMS/Telegram alerts to patients when their queue status changes (e.g., "Your medication is ready for pickup").
* **Clinic Analytics Dashboard:** Calculating time-deltas between pipeline states to identify operational bottlenecks and optimize average wait times.
