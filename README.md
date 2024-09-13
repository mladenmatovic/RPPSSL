# Rock Paper Scissors Spock Lizard Game

Welcome to the Rock Paper Scissors Spock Lizard game! This interactive web application allows you to play the classic game with a twist, either against the computer or in real-time multiplayer matches.

# Features

## Home Page

Test API endpoints
Play against the computer
View and reset scoreboard (max 10 results)

## API Endpoints

/choices: Retrieves available game choices
/play: Initiates a game against the computer
/choice: Gets random option

## Gameplay

Single-player mode against the computer
Real-time multiplayer matches

## Multiplayer Lobby

View active game rooms
Join existing rooms
Create new rooms

## Scoreboard

Track game results
Resetable
Displays up to 10 recent results


# Project Structure 
The project is built using a microservices architecture:

## Frontend
* React application

## Backend Microservices:

* RandomNumber Service
* Identity Service
* Game Service

Each component (frontend and each microservice) has its own Dockerfile for containerization.

# How to Run

1. Ensure you have Docker and Docker Compose installed on your system.
2. Clone the repository to your local machine.
3. Navigate to the project root directory.
4. Run the following command to build and start all services:

	`docker-compose up --build`

5. Once all services are up and running, access the game at http://localhost:3000

## Running Tests

You can run these tests from the command line interface (CLI) using the following steps:

1. Open a terminal or command prompt.
2. Navigate to the root directory of the project.
3. To run all tests in both projects, use the following command:

	`dotnet test`

4. To run tests for a specific project, use:

	`dotnet test src\Services\RPSSL.GameService.Tests\RPSSL.GameService.Tests.csproj`
	
	`dotnet test src\Services\RPSSL.RandomNumberService.Tests\RPSSL.RandomNumberService.Tests.csproj`
	
5. The test results will be displayed in the console, showing which tests passed or failed.

# How to Play

## Single-player Mode

1. On the home page, enter your root URL in the designated field.
2. Click to populate the choices from the /choices endpoint.
3. Select a choice to play against the computer using the /play endpoint.
4. Get random choice from the /choice endpoint.

## Multiplayer Mode

1. Register for an account or log in if you already have one.
2. Navigate to the lobby.
3. Join an existing room or create a new one.
4. Play against other online players in real-time.

# Technologies Used

* Frontend: React
* Backend: ASP.NET Core microservices
* Real-time Communication: SignalR
* Containerization: Docker
* Container Orchestration: Docker Compose