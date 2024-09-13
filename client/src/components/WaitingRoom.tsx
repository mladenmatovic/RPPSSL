import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';
import { Card, CardHeader, CardContent } from "./ui/card"
import { Button } from "./ui/button"
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

interface WaitingRoomProps {
  token: string | null;
}

const WaitingRoom: React.FC<WaitingRoomProps> = ({ token }) => {
  const [players, setPlayers] = useState<string[]>([]);
  const { roomId } = useParams<{ roomId: string }>();
  const [connection, setConnection] = useState<HubConnection>();
  const navigate = useNavigate();

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

          connection.on("PlayerJoined", (player) => {
            setPlayers(prevPlayers => [...prevPlayers, player]);
          });

          connection.on("PlayerLeft", (player) => {
            setPlayers(prevPlayers => prevPlayers.filter(p => p !== player));
          });

          connection.on("GameStarted", (gameId) => {
            navigate(`/room/${roomId}/${gameId}`);
          });
        })
        .catch(error => console.log('Error connecting to SignalR Hub:', error));

      return () => {
        connection.invoke("LeaveRoom", roomId);
        connection.stop();
      };
    }
  }, [connection, roomId, navigate]);

  const leaveRoom = async () => {
    if (connection) {
      await connection.invoke("LeaveRoom", roomId);
      navigate('/lobby');
    }
  };

  return (
    <div className="max-w-md mx-auto">
      <Card>
        <CardHeader>Waiting for Opponent</CardHeader>
        <CardContent>
          <p>Room ID: {roomId}</p>
          <p>Waiting for an opponent to join...</p>
          <Button onClick={leaveRoom} className="mt-4">Leave Room</Button>
        </CardContent>
      </Card>
    </div>
  );
};

export default WaitingRoom;