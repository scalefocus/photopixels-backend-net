## How to run the project

### Prerequisites
- Docker with docker compose installed

### Steps

1. Clone this repository
2. Navigate to the root of the repository
3. Make a copy of `.env.dev` and rename it to `.env`
4. Fill the `.env` file with the required values
5. Run `docker-compose up`
6. Wait for the containers to start

To stop the containers run `docker-compose down`

To stop the containers and reset the database, run `docker-compose down -v`

