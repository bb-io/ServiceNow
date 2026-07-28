# Blackbird.io ServiceNow

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

ServiceNow is a cloud platform for IT service management and knowledge management. This app lets you work with ServiceNow knowledge articles and incidents directly from your Blackbird workflows — search and read articles, create and update them, open and manage incidents, add comments, and download attachments.

## Before setting up

Before you can connect, you need:

- A ServiceNow instance (for example `https://your-instance.service-now.com`).
- The **Knowledge API** application (`sn_km_api`) installed on the instance. See [Installing the Knowledge API](#installing-the-knowledge-api).
- A ServiceNow user account with permission to read and write knowledge articles and incidents through the REST APIs (Table API, Knowledge Management API and Attachment API).
- The username and password of that account.

## Installing the Knowledge API

The article actions require the ServiceNow **Knowledge API** application (`sn_km_api`). This application might not be installed on your ServiceNow instance by default.

A ServiceNow administrator can install it as follows:

1. Log in to the ServiceNow instance as an administrator.
2. Navigate to **All → Admin Center → Application Manager → Available for you**.
   
   On some ServiceNow versions, use **All → System Applications → All Available Applications → All**.
3. Search for **Knowledge API**. If it does not appear, search for its application ID: `sn_km_api`.
4. Open the **Knowledge API** application and click **Install**.
5. Wait for the installation to complete.
6. To verify the installation:
   - Open **REST API Explorer**.
   - Check that `sn_km_api` is available in the **Namespace** list.

## Connecting

1. Navigate to Apps and search for **ServiceNow**.
2. Click **Add connection**.
3. Name your connection for future reference, for example "My ServiceNow account".
4. Fill in the connection fields:
   - **Instance URL** — the base address of your instance, for example `https://your-instance.service-now.com`.
   - **Username** — your ServiceNow username.
   - **Password** — your ServiceNow password.
5. Click **Connect**.
6. Confirm that the connection has appeared and the status is **Connected**.

![Connection](image/README/connection.png)

## Actions

### Articles

- **Search articles** — Find knowledge articles matching a search text and optional filters (language, knowledge bases, state, and created/updated date ranges). Returns the matching articles with their ID, number, title, body preview, language, state, knowledge base and created/updated timestamps. Articles in every workflow state are searchable, drafts included.
- **Get article metadata** — Read all metadata fields of a single article (title, state, knowledge base, category, author, language, body and timestamps).
- **Update article metadata** — Change one or more fields of an article (title, body, knowledge base, category, language).
- **Download article** — Export an article's translatable fields (title and body) as a self-describing HTML file ready for translation.
- **Upload article** — Import a translated file (`.html`/`.xliff`/`.xlf`) and write its title and body onto the article of the selected language.
- **Create article** — Create a new article with a title, language, knowledge base and optional HTML body.

> Note: newly created and updated articles are kept in the **draft** state. Publishing an article typically goes through your knowledge base's publish workflow and may require an approval, so the "Is draft = off" (publish) option is best-effort and depends on your instance configuration.

#### Translations

ServiceNow keeps every language of an article in its own `kb_knowledge` record, linked to the source article through its **Parent** field — each language therefore has its own article number, workflow state and lifecycle.

**Upload article** follows that model: it writes into the record for the language you select, creating and linking that record to the source article the first time you upload a given language. The source article is never overwritten with translated content. The output exposes both the source article ID (`Root article ID`) and the language variant that was written (`Translated article ID`).

Only languages that are **active** on your instance can be used. ServiceNow silently falls back to the instance default for an inactive language code, so the action rejects one instead; activate the language under *System Localization → Languages* first. The **Language** inputs list only your active languages.

### Incidents

- **Create incident** — Open a new incident with a short description and optional details, urgency, impact, caller and assignee.
- **Get incident** — Retrieve a single incident by its ID.
- **Update incident** — Change fields on an incident (description, state, priority, urgency, impact, assignee).
- **Delete incident** — Permanently remove an incident.
- **Search incidents** — Find incidents matching a ServiceNow encoded query (for example `state=2^priority=1`). Pagination is handled automatically.
- **Get incident comments** — List the customer-visible comments on an incident, newest first, with author and timestamp.
- **Add comment to incident** — Append a customer-visible comment to an incident.

### Attachments

- **Download attachment** — Download the file content of an attachment by its ID.

## Events

### Articles

- **On articles created or updated** — Triggers for knowledge articles created or updated since the previous poll. Optional filters: article ID (watch a single article), language, and knowledge bases. Each returned article is tagged as *created* or *updated*.
- **On articles status changed** — Triggers when an article's workflow state (draft, review, published, retired) changes. Optional filters: article ID, language, knowledge bases, and status (only trigger when the article changes *into* one of the selected states). Reports the previous and new state.

### Incidents

- **On new incidents** — Triggers for incidents opened since the previous poll.
- **On new incident comment** — Triggers for customer-visible incident comments added since the previous poll, each with the incident it belongs to. New comments are read in a single query across all incidents, so cost does not grow with the number of incidents.

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
