import React, { useState } from 'react';
import { Button } from "./ui/button"
import { Input } from "./ui/input"
import { Card, CardHeader, CardContent } from "./ui/card"
import { Alert, AlertTitle, AlertDescription } from "./ui/alert"
import { Egg, File, Scissors, HandMetal, Bug } from 'lucide-react'

interface Choice {
    id: number;
    name: string;
  }

interface GameResult {
  results: string;
  player: number;
  computer: number;
}


interface PlayRequest
{
    player: number
}

const RPSLSGame: React.FC = () => {
  const [baseUrl, setBaseUrl] = useState<string>('');
  const [choices, setChoices] = useState<Choice[]>([]);
  const [result, setResult] = useState<GameResult | null>(null);
  const [randomChoice, setRandomChoice] = useState<Choice | null>(null);
  const [scoreboard, setScoreboard] = useState<GameResult[]>([]);
  const [error, setError] = useState<string | null>(null);


  const getChoiceName = (id: number): string => {
    const choice = choices.find(c => c.id === id);
    return choice ? choice.name : `Unknown (${id})`;
  };

  const fetchChoices = async (): Promise<void> => {
    try {
      const response = await fetch(`${baseUrl}/choices`);
      if (!response.ok) throw new Error('Failed to fetch choices');
      const data: Choice[] = await response.json();
      setChoices(data);
      setError(null);
    } catch (error) {
      setError('Failed to fetch choices. Please check the base URL and CORS settings.');
    }
  };

  const playGame = async (choiceId: number): Promise<void> => {
    try {
      const playRequest: PlayRequest = { player: choiceId };
      const response = await fetch(`${baseUrl}/play`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(playRequest),
      });
      if (!response.ok) throw new Error('Failed to play game');
      const data: GameResult = await response.json();
      setResult(data);
      updateScoreboard(data);
      setError(null);
    } catch (error) {
      setError('Failed to play game. Please try again.');
    }
  };

  const getRandomChoice = async (): Promise<void> => {
    try {
      const response = await fetch(`${baseUrl}/choice`);
      if (!response.ok) throw new Error('Failed to get random choice');
      const data: Choice = await response.json();
      setRandomChoice(data);
      setError(null);
    } catch (error) {
      setError('Failed to get random choice. Please try again.');
    }
  };

  const updateScoreboard = (newResult: GameResult): void => {
    setScoreboard(prevScoreboard => {
      const updatedScoreboard = [newResult, ...prevScoreboard].slice(0, 10);
      return updatedScoreboard;
    });
  };

  const resetScoreboard = (): void => {
    setScoreboard([]);
  };

  const getResultColor = (result: string): string => {
    switch (result) {
      case 'win':
        return 'border-green-200';
      case 'lose':
        return 'border-red-200';
      case 'tie':
        return 'border-gray-200';
      default:
        return '';
    }
  };

  const getChoiceIcon = (name: string) => {
    switch (name.toLowerCase()) {
      case 'rock':
        return <Egg size={24} />;
      case 'paper':
        return <File size={24} />;
      case 'scissors':
        return <Scissors size={24} />;
      case 'lizard':
        return <Bug size={24} />; // Using Bug as a substitute for Lizard
      case 'spock':
        return <HandMetal size={24} />; // Using HandMeal as a substitute for Spock
      default:
        return null;
    }
  };

  return (
    <div className="p-4 max-w-md mx-auto">  
      <h2 className="text-xl font-semibold mb-4">
          Step 1: Put your root URL here
      </h2>    
      <Input
        type="text"
        value={baseUrl}
        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setBaseUrl(e.target.value)}
        placeholder="Enter base URL"
        className="mb-4"
      />

      <h2 className="text-xl font-semibold mb-4">
        Step 2: Populate choices from the /choices endpoint
      </h2>

      <Button onClick={fetchChoices} className="w-full mb-4">Populate Choices</Button>
      <div className="grid grid-cols-2 gap-2 mb-4">
        {choices.map((choice) => (
          <Button key={choice.id} onClick={() => playGame(choice.id)} variant="outline">
            {choice.name}
          </Button>
        ))}
      </div>

      <h2 className="text-xl font-semibold mb-4">
            Step 3: Click an above choice to play against the computer with the /play endpoint
      </h2>

      {result && (
        <Card className={`mb-4 ${getResultColor(result.results)}`}>
          <CardHeader></CardHeader>
          <CardContent>
            <p>
              You played {getChoiceName(result.player)} & the computer played {getChoiceName(result.computer)}.
              You {result.results}.
            </p>
          </CardContent>
        </Card>
      )}
      
      <Button onClick={getRandomChoice} className="w-full mb-4">Random Choice</Button>
      {randomChoice && (
        <div className="mb-4 flex items-center justify-center">
        {getChoiceIcon(randomChoice.name)}
        <span className="ml-2"> {randomChoice.name}</span>
      </div>
      )}
      
      {error && (
        <Alert variant="destructive" className="mb-4">
          <AlertTitle>Error</AlertTitle>
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}
      
      <Card>
        <CardHeader className="flex justify-between items-center">
          <h2 className="text-xl font-bold">Scoreboard</h2>
          <Button onClick={resetScoreboard} variant="destructive">Reset</Button>
        </CardHeader>
        <CardContent>
          {scoreboard.map((score, index) => (
            <div key={index} className={`mb-2 p-2 border rounded ${getResultColor(score.results)}`}>
              <p>Player: {getChoiceName(score.player)}, Computer: {getChoiceName(score.computer)}, Outcome: {score.results}</p>
            </div>
          ))}
        </CardContent>
      </Card>
    </div>
  );
};

export default RPSLSGame;