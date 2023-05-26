using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public CharacterStats playStats;

    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    public void RigisterPlayer(CharacterStats player)
    {
        playStats = player;
    }

    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    public void NotifyObserver()
    {
        foreach(var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }
}

