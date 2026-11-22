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

## Story 3: Create a new ticket

> As a helpdesk agent, I can create a new ticket, so that a reported problem is recorded and can be worked on.

| ID | Steps | Expected result | Automated by |
|---|---|---|---|
| TC-3.1 | Click "+ New Ticket", enter a title and description, pick an assignee, click "Save Ticket" | The new ticket's detail page opens; status is Open; created and updated times are equal and in UTC | `CreateAsync_SetsOpenStatusAndUtcTimestamps` |
| TC-3.2 | Create a ticket leaving "Assign To" as "-- Unassigned --" | The ticket is saved and shows "Unassigned" | `CreateAsync_AllowsUnassignedTicket` |
| TC-3.3 | Click "Save Ticket" with an empty title | "Title is required." is shown; nothing is saved | (visual check) |
| TC-3.4 | Enter a 2-character title and save | "Title must be between 3 and 150 characters." is shown | (visual check) |
| TC-3.5 | Click "Save Ticket" with an empty description | "Description is required." is shown | (visual check) |
| TC-3.6 | Enter a title with leading and trailing spaces and save | The saved title has no surrounding spaces | `CreateAsync_TrimsTitleAndDescription` |
| TC-3.7 | Click "Cancel" on the form | The list page opens; nothing is saved | (visual check) |
| TC-3.8 | Call `POST /api/tickets` with a valid body | 201 Created, a `Location` header pointing at the new ticket, and the ticket as JSON | (integration test, to be added) |
| TC-3.9 | Call `POST /api/tickets` with an empty title | 400 Bad Request listing the validation errors | (integration test, to be added) |
| TC-3.10 | Call `POST /api/tickets` with `assignedUserId` 9999 | 400 Bad Request, "Assigned user with ID 9999 was not found."; no ticket is created | `CreateAsync_ThrowsArgumentException_WhenAssignedUserDoesNotExist` |
| TC-3.11 | Call `GET /api/users` | 200 OK with the three users ordered by name | `GetAllAsync_ReturnsSeededUsersOrderedByName` |

## Story 4: Update a ticket's details and status

> As a helpdesk agent, I can update a ticket's details and status, so that the ticket reflects the current state of the work.

| ID | Steps | Expected result | Automated by |
|---|---|---|---|
| TC-4.1 | Open a ticket, click "Edit" | The edit form opens with the current title, description and status filled in | (visual check) |
| TC-4.2 | Change the title, description and status, click "Save Changes" | The detail page shows the new values; "Last Updated" is later than before; "Created" is unchanged | `UpdateAsync_ChangesDetailsAndStatus_AndAdvancesUpdatedAtOnly` |
| TC-4.3 | Edit a ticket that has an assignee and save | The assignee is unchanged | `UpdateAsync_KeepsExistingAssignee` |
| TC-4.4 | Clear the title and save | "Title is required." is shown; nothing is saved | (visual check) |
| TC-4.5 | Click "Cancel" on the edit form | The detail page opens; nothing is changed | (visual check) |
| TC-4.6 | Open `/tickets/9999/edit` | "Ticket #9999 was not found" is shown | `UpdateAsync_ReturnsNull_WhenTicketDoesNotExist` |
| TC-4.7 | Call `PUT /api/tickets/1` with a valid body | 200 OK with the updated ticket | (integration test, to be added) |
| TC-4.8 | Call `PUT /api/tickets/1` with `"status": "Done"` | 400 Bad Request | (integration test, to be added) |
| TC-4.9 | Call `PUT /api/tickets/9999` with a valid body | 404 Not Found | (integration test, to be added) |

## Story 5: Assign a ticket to a user

> As a helpdesk agent, I can assign a ticket to a user, so that it is clear who is responsible for it.

| ID | Steps | Expected result | Automated by |
|---|---|---|---|
| TC-5.1 | Open a ticket, pick a different user under "Assign To", click "Update Assignee" | "Assigned To" shows the new user, a confirmation message appears, and "Last Updated" moves forward | `AssignAsync_ReassignsTicketAndAdvancesUpdatedAt` |
| TC-5.2 | Pick "-- Unassigned --" and click "Update Assignee" | "Assigned To" shows "Unassigned" | `AssignAsync_WithNull_UnassignsTicket` |
| TC-5.3 | Open a ticket without changing the dropdown | "Update Assignee" is disabled until a different user is picked | (visual check) |
| TC-5.4 | Go back to the list after assigning | The list shows the new assignee | (visual check) |
| TC-5.5 | Call `PATCH /api/tickets/1/assign` with `{"assignedUserId": 2}` | 200 OK with the ticket assigned to Bob Martinez | (integration test, to be added) |
| TC-5.6 | Call `PATCH /api/tickets/1/assign` with `{"assignedUserId": 9999}` | 400 Bad Request; the previous assignee is kept | `AssignAsync_ThrowsArgumentException_AndKeepsAssignee_WhenUserDoesNotExist` |
| TC-5.7 | Call `PATCH /api/tickets/9999/assign` | 404 Not Found | `AssignAsync_ReturnsNull_WhenTicketDoesNotExist` |
