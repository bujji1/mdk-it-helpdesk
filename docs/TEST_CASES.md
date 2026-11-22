# Acceptance Test Cases

Manual test cases, grouped by user story. Each story also has automated tests; the last column of each table names them.

Seed data used by these cases: users Alice Johnson, Bob Martinez and Priya Patel; tickets #1 (Open, Alice), #2 (In Progress, Bob) and #3 (Closed, Priya).

## Story 1: See a list of all tickets

> As a helpdesk agent, I can see a list of all tickets, so that I know what needs attention.

| ID | Steps | Expected result | Automated by |
|---|---|---|---|
| TC-1.1 | Open the home page `/` | A table lists the three seeded tickets with ID, title, status, assignee, created and updated times | `GetTicketsAsync_ReturnsSeededTicketsWithAssigneeNames` |
| TC-1.2 | Look at the Status column | Open, In Progress and Closed are shown as distinct coloured badges | (visual check) |
| TC-1.3 | Look at the order of the rows | The most recently updated ticket is first | `GetTicketsAsync_ReturnsMostRecentlyUpdatedFirst` |
| TC-1.4 | View a ticket that has no assignee | The Assigned To column shows "Unassigned" | `GetTicketsAsync_ReturnsNullAssigneeName_WhenTicketIsUnassigned` |
| TC-1.5 | Call `GET /api/tickets` (from `/swagger`) | 200 OK with a JSON array; `status` is a readable string such as `"InProgress"`; timestamps are UTC | (integration test, to be added) |
| TC-1.6 | Open `/swagger` | Swagger UI loads and lists `GET /api/tickets` with its description | (visual check) |

## Story 2: Open a ticket to see its details

> As a helpdesk agent, I can open a ticket to see its full details and timestamps, so that I understand the problem before working on it.

| ID | Steps | Expected result | Automated by |
|---|---|---|---|
| TC-2.1 | On the list page, click a ticket's title | The detail page `/tickets/{id}` opens for that ticket | (visual check) |
| TC-2.2 | View the detail page for ticket #2 | Title, description, status "In Progress", assignee "Bob Martinez", created and last-updated times (UTC) are shown | `GetByIdAsync_ReturnsTicketWithAssigneeAndTimestamps` |
| TC-2.3 | Click "Back to List" | The list page opens | (visual check) |
| TC-2.4 | Open `/tickets/9999` | A "Ticket #9999 was not found" message is shown with a link back to the list | `GetByIdAsync_ReturnsNull_WhenTicketDoesNotExist` |
| TC-2.5 | Call `GET /api/tickets/2` | 200 OK with the ticket as JSON | (integration test, to be added) |
| TC-2.6 | Call `GET /api/tickets/9999` | 404 Not Found | (integration test, to be added) |
