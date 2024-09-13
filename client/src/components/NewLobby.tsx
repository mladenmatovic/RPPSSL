import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';
import { Button } from "./ui/button"

interface LobbyProps {
  token: string | null;
}

interface Room {
  id: string;
  playerCount: number;
}

const NewLobby: React.FC<LobbyProps> = ({ token }) => {
  const [rooms, setRooms] = useState<Room[]>([]);
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl("https://localhost:7267/gamehub", {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
  }, [token]);

  useEffect(() => {
    if (connection) {
      connection.start()
        .then(() => {
          console.log('SignalR Connected');
          connection.invoke('GetRooms');

          connection.on('ReceiveRooms', (updatedRooms: Room[]) => {
            setRooms(updatedRooms);
          });

          connection.on('RoomCreated', (newRoom: Room) => {
            debugger;
            setRooms(prevRooms => [...prevRooms, newRoom]);
          });

          connection.on('RoomUpdated', (updatedRoom: Room) => {
            setRooms(prevRooms => prevRooms.map(room => 
              room.id === updatedRoom.id ? updatedRoom : room
            ));
          });

          connection.on('RoomArchived', (roomId: string) => {
            setRooms(prevRooms => prevRooms.filter(room => room.id !== roomId));
          });
        })
        .catch(error => console.error('SignalR Connection Error: ', error));
    }

    return () => {
      if (connection) {
        connection.stop();
      }
    };
  }, [connection]);

  const createRoom = useCallback(async () => {
    if (connection) {
      try {
        const roomId = await connection.invoke('CreateRoom');
        navigate(`/room/${roomId}`);
      } catch (error) {
        console.error('Failed to create room:', error);
      }
    }
  }, [connection, navigate]);

  const joinRoom = useCallback(async (roomId: string) => {
    if (connection) {
      try {
        //await connection.invoke('JoinRoom', roomId);
        navigate(`/room/${roomId}`);
      } catch (error) {
        console.error('Failed to join room:', error);
      }
    }
  }, [connection, navigate]);

  return (
    <div className="max-w-md mx-auto">
      <h2 className="text-2xl mb-4">Game Lobby</h2>
      <Button onClick={createRoom} className="mb-4 bg-green-500 text-white py-2 px-4 rounded hover:bg-green-600">
        Create Room
      </Button>
      <h3 className="text-xl mb-2">Available Rooms:</h3>
      <ul>
        {rooms.map(room => (
          <li key={room.id} className="mb-2 p-2 border rounded">
            Room {room.id} - Players: {room.playerCount}
            <Button 
                onClick={() => joinRoom(room.id)}
                className={`ml-2 py-1 px-2 rounded ${
                room.playerCount < 2
                    ? 'bg-blue-500 text-white hover:bg-blue-600'
                    : 'bg-gray-300 text-gray-500 cursor-not-allowed'
                }`}
                disabled={room.playerCount >= 2}
            >
                {room.playerCount < 2 ? 'Join' : 'Full'}
            </Button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default NewLobby;