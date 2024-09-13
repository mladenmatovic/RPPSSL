import { useState, useEffect, useCallback } from 'react';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';

export const useSignalR = (url: string, token: string) => {
  const [connection, setConnection] = useState<HubConnection | null>(null);

  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);
    debugger;
    newConnection.start()
      .then(() => console.log('Connected to SignalR Hub'))
      .catch(err => console.error('Error connecting to SignalR Hub:', err));

    return () => {
      if (newConnection) {
        newConnection.stop();
      }
    };
  }, [url, token]);

  const sendMessage = useCallback(async (methodName: string, ...args: any[]) => {
    if (connection) {
      try {
        await connection.invoke(methodName, ...args);
      } catch (err) {
        console.error(`Error calling ${methodName}:`, err);
      }
    } else {
      console.log("No connection to server yet.");
    }
  }, [connection]);

  const addSignalREventListener = useCallback((eventName: string, newMethod: (...args: any[]) => void) => {
    if (connection) {
      connection.on(eventName, newMethod);
    }
  }, [connection]);

  const removeSignalREventListener = useCallback((eventName: string, method: (...args: any[]) => void) => {
    if (connection) {
      connection.off(eventName, method);
    }
  }, [connection]);

  return { connection, sendMessage, addSignalREventListener, removeSignalREventListener };
};