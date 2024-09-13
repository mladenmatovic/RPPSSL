import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useNavigate  } from 'react-router-dom';
import { Button } from "./ui/button"
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';

interface LobbyProps {
  token: string | null;
}

interface Room {
  id: string;
  playerCount: number;
}

const Lobby: React.FC<LobbyProps> = ({ token }) => {
  const [rooms, setRooms] = useState<Room[]>([]);
  const navigate = useNavigate();

  useEffect(() => {
    fetchRooms();
  }, []);

  const fetchRooms = async () => {
    try {
      const response = await axios.get('https://localhost:7267/api/gamelobby/list', {
        headers: { Authorization: `Bearer ${token}` }
      });
      setRooms(response.data);
    } catch (error) {
      console.error('Failed to fetch rooms:', error);
    }
  };

  const createRoom = async () => {
    try {
      const response = await axios.post('https://localhost:7267/api/gamelobby/create', {}, {
        headers: { Authorization: `Bearer ${token}` }
      });
      debugger;
      navigate(`/room/${response.data.roomId}`);
    } catch (error) {
      console.error('Failed to create room:', error);
    }
  };

  const joinRoom = async (roomId: string) => {
    /*try {
      await axios.post(`https://localhost:7267/api/gamelobby/join/${roomId}`, {}, {
        headers: { Authorization: `Bearer ${token}` }
      });
      navigate(`/room/${roomId}`);
    } catch (error) {
      console.error('Failed to join room:', error);
    }*/
      navigate(`/room/${roomId}`);
  };

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
              className="ml-2 bg-blue-500 text-white py-1 px-2 rounded hover:bg-blue-600"
            >
              Join
            </Button>
          </li>
        ))}
      </ul>
    </div>
  );
};

export default Lobby;