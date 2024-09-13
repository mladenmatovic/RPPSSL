import React, { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { Button } from "./ui/button"
import { Card, CardHeader, CardContent } from "./ui/card"
import { Alert, AlertTitle, AlertDescription } from "./ui/alert"
import { useNavigate } from 'react-router-dom';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

interface MultiplayerGameProps {
    token: string | null;
    currentUserId: string; 
  }

interface Choice {
  id: number;
  name: string;
}

export enum GameStatus {
    WaitingForPlayers = 0,
    InProgress = 1,
    Completed = 2
  }

interface GameResult {    
    playerMove: number;
    opponentMove: number;
    result: string;
  }

export interface GameState {
    id: string;
    roomId: string;
    player1Id: string;
    player2Id: string;
    player1Move?: number;
    player2Move?: number;
    status: GameStatus;
    winnerId?: string;
    createdAt: string;
    completedAt?: string;
  }

export function isPlayer1(gameState: GameState, currentPlayerId: string): boolean {
    return gameState.player1Id === currentPlayerId;
  }
  
  export function getCurrentPlayerMove(gameState: GameState, currentPlayerId: string): number | undefined {
    return isPlayer1(gameState, currentPlayerId) ? gameState.player1Move : gameState.player2Move;
  }
  
  export function getOpponentMove(gameState: GameState, currentPlayerId: string): number | undefined {
    return isPlayer1(gameState, currentPlayerId) ? gameState.player2Move : gameState.player1Move;
  }
  
  export function getOpponentId(gameState: GameState, currentPlayerId: string): string {
    return isPlayer1(gameState, currentPlayerId) ? gameState.player2Id : gameState.player1Id;
  }
  
  export function getWinResult(gameState: GameState, currentPlayerId: string): string {
    let result: 'win' | 'lose' | 'draw';
    if (gameState.winnerId === currentPlayerId) {
      result = 'win';
    } else if (gameState.winnerId === getOpponentId(gameState, currentPlayerId)) {
      result = 'lose';
    } else {
      result = 'draw';
    }
    return result;
  }

  function createGameResult(gameState: GameState, currentPlayerId: string): GameResult | null {
    const playerMove = getCurrentPlayerMove(gameState, currentPlayerId);
    const opponentMove = getOpponentMove(gameState, currentPlayerId);
  
    if (playerMove === undefined || opponentMove === undefined) {
      return null; // Both moves must be defined to create a result
    }
  
    let result = getWinResult(gameState, currentPlayerId);
  
    return { playerMove, opponentMove, result };
  }

const MultiplayerGame: React.FC<MultiplayerGameProps> = ({ token, currentUserId }) => {
  const [gameState, setGameState] = useState<GameState | null>(null);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const [connection, setConnection] = useState<HubConnection>();
  const { roomId } = useParams<{ roomId: string; }>();
  const [currentGameId, setCurrentGameId] = useState<string>();
  const [wantsNewGame, setWantsNewGame] = useState(false);
  const [opponentWantsNewGame, setOpponentWantsNewGame] = useState(false);
  const [scoreboard, setScoreboard] = useState<GameResult[]>([]);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl("https://localhost:7267/gameHub", {
          accessTokenFactory: () => localStorage.getItem('token') || ''
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, [token]);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('Connected to SignalR Hub');
          connection.invoke("JoinRoom", roomId);

          connection.on("JoinedRoom", (initialState: GameState | null) => {            
            if (!initialState) {
              const newState: GameState = {   
                id: '',             
                roomId: roomId ? roomId : '',
                player1Id: currentUserId,
                player2Id: 'Waiting for a player...',
                status: GameStatus.WaitingForPlayers,
                createdAt: ''
              };
              setGameState(newState);
              setCurrentGameId(newState.id);
            } else {
              setGameState(initialState);
              setCurrentGameId(initialState.id);
            }
          });

          connection.on("PlayerJoined", (player) => {
            console.log(`${player} joined the game`);          
          });

          connection.on("PlayerLeft", (player) => {
            console.log(`${player} left the game`);

            setGameState(prevState => {
              if (prevState) {
                let updatedState: GameState = { ...prevState };
                
                if (isPlayer1(prevState, player)) {
                  // If player 1 left, make player 2 the new player 1
                  updatedState.player1Id = prevState.player2Id || '';
                  updatedState.player1Move = undefined;
                  updatedState.player2Id = 'Waiting for a player...';
                  updatedState.player2Move = undefined;
                } else {
                  // If player 2 left, just set player2Id to null
                  updatedState.player2Id = 'Waiting for a player...';
                  updatedState.player2Move = undefined;
                  updatedState.player1Move = undefined;
                }

                // Update game status
                updatedState.status = GameStatus.WaitingForPlayers;

                return updatedState;
              }
              return prevState;
            });         
          }); 

          connection.on("GameCreated", (initialState: GameState) => {
            setCurrentGameId(initialState.id);
            setGameState(initialState);
          });

          connection.on("PlayerWantsNewGame",(playerId: string) => {
            if (playerId !== currentUserId) {
              setOpponentWantsNewGame(true);
            }
          })

          connection.on("NewGameStarted", (newGameState: GameState) => {
            setCurrentGameId(newGameState.id);
            setGameState(newGameState);
            setWantsNewGame(false);
            setOpponentWantsNewGame(false);
          });

          connection.on("GameStateUpdated", (updatedState: GameState) => {
            debugger;
            setCurrentGameId(updatedState.id);
            setGameState(updatedState);
            if (updatedState.player1Move && updatedState.player2Move) {
              const gameResult = createGameResult(updatedState, currentUserId);
              if (gameResult) {
                updateScoreboard(gameResult);
              }
            }
          });
        })
        .catch(error => console.log('Error connecting to SignalR Hub:', error));

      return () => {
        connection.stop();
      };
    }
  }, [connection, roomId]); 

  const GAME_CHOICES: Choice[] = [
    { id: 1, name: 'Rock' },
    { id: 2, name: 'Paper' },
    { id: 3, name: 'Scissors' },
    { id: 4, name: 'Spock' },
    { id: 5, name: 'Lizard' },
  ];

  const makeMove = async (moveId: number) => {
    debugger;
    if (connection) {
      await connection.invoke("MakeMove", currentGameId, moveId);
    }
  };

  const leaveGame = async () => {
    if (connection) {
      await connection.invoke("LeaveRoom", roomId);
      navigate('/lobby');
    }
  };

  const requestNewGame = async () => {
    if (connection) {
      setWantsNewGame(true);
      await connection.invoke("RequestNewGame", roomId);
      
      if (opponentWantsNewGame) {
        await connection.invoke("StartNewGame", roomId);
      }
    }
  };

  const getResultColor = (result: string) => {
    switch (result.toLowerCase()) {
      case 'win': return 'bg-green-100';
      case 'lose': return 'bg-red-100';
      default: return 'bg-gray-100';
    }
  };

  const updateScoreboard = (newResult: GameResult): void => {
    setScoreboard(prevScoreboard => {
      const updatedScoreboard = [newResult, ...prevScoreboard].slice(0, 10);
      return updatedScoreboard;
    });
  };

  const getChoiceName = (id: number): string => {
    const choice = GAME_CHOICES.find(c => c.id === id);
    return choice ? choice.name : `Unknown (${id})`;
  };

  if (!gameState) {
    return <div className="p-4 max-w-md mx-auto">Waiting for other player...</div>;
  }

  const currentPlayerMove = getCurrentPlayerMove(gameState, currentUserId);
  const opponentMove = getOpponentMove(gameState, currentUserId);
  const opponentId = getOpponentId(gameState, currentUserId);

  return (
    <div className="p-4 max-w-md mx-auto">
      <h2 className="text-2xl mb-4">Multiplayer Mode</h2>
      <Card className="mb-4">
        <CardHeader>Game State</CardHeader>
        <CardContent>
          <p>Room: {gameState.roomId}</p> 
          <p>You: {currentUserId}</p>
          <p>Opponent: {opponentId}</p>
          {currentPlayerMove && <p>Your Move: {getChoiceName(currentPlayerMove)}</p>}
          {gameState.status === GameStatus.Completed && opponentMove && <p>Opponent's Move: {getChoiceName(opponentMove)}</p>}
          {gameState.status === GameStatus.Completed && (
            <p className={`mt-2 p-2 rounded ${getResultColor(getWinResult(gameState, currentUserId))}`}>
              Result: {getWinResult(gameState, currentUserId)!}
            </p>
          )}
        </CardContent>
      </Card>
      
      {gameState.status === GameStatus.InProgress && !currentPlayerMove && (
        <div className="grid grid-cols-2 gap-2 mb-4">
          {GAME_CHOICES.map((choice) => (
            <Button 
              key={choice.id} 
              onClick={() => makeMove(choice.id)} 
              variant="outline"
            >
              {choice.name}
            </Button>
          ))}
        </div>
      )}

    {gameState.status === GameStatus.Completed && (
        <Button 
          onClick={requestNewGame} 
          className="w-full bg-green-500 text-white py-2 rounded hover:bg-green-600 mb-4"
          disabled={wantsNewGame}
        >
          {wantsNewGame ? 'Waiting for opponent...' : 'New Game'}
        </Button>
      )}

      {opponentWantsNewGame && !wantsNewGame && (
        <Alert variant="default" className="mb-4">
          <AlertTitle>New Game Request</AlertTitle>
          <AlertDescription>Your opponent wants to play again. Click 'New Game' to accept.</AlertDescription>
        </Alert>
      )}

      <Button onClick={leaveGame} className="w-full bg-red-500 text-white py-2 rounded hover:bg-red-600">
        Leave Room
      </Button>

      {error && (
        <Alert variant="destructive" className="mt-4">
          <AlertTitle>Error</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <Card className="mb-4">
          <CardHeader className="flex justify-between items-center">
            <h2 className="text-xl font-bold">Scoreboard</h2>
          </CardHeader>
          <CardContent>
            {scoreboard.map((score, index) => (
              <div key={index} className={`mb-2 p-2 border rounded ${getResultColor(score.result)}`}>
                <p>You played {getChoiceName(score.playerMove)}, Opponent played {getChoiceName(score.opponentMove)} - {score.result.toUpperCase()}</p>
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    );
};

export default MultiplayerGame;