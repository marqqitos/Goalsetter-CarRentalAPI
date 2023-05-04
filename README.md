# Goalsetter-CarRentalAPI

## Decisions

- Clients can create multiple rentals for the same day
- A vehicle cant be removed if it's rented (or will be in the future), rentals must be cancelled first
- A client cant be removed if it's rented (or will be in the future), rentals must be cancelled first
- Deleted vehicles will be marked as inactive on the database
- Deleted clients will be marked as inactive on the database
- Cancelled rentals will be marked as inactive on the database