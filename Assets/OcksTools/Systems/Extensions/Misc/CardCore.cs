using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string Name = "";
    public Card(string name)
    {
        Name = name;
    }
    public bool Matches(string a)
    {
        return Name == a;
    }
}
public class Deck
{
    public CardCollection BaseCards = new CardCollection(0);
    public CardCollection CurrentPile = new CardCollection(0);
    public CardCollection DiscardPile = new CardCollection(0);

    public void Shuffle()
    {
        CurrentPile.Shuffle();
    }
    public void RefreshFromBase()
    {
        CurrentPile = new CardCollection(BaseCards);
        DiscardPile.Clear();
        Shuffle();
    }
    public void AddDiscardsToBottom(Card c)
    {
        var d = DiscardPile.Shuffle();
        var sd = d.Cards.CombineLists(CurrentPile.Cards);
        CurrentPile = new CardCollection(sd);
    }
}

public class Hand
{
    public List<Card> CurrentHand = new List<Card>();
    public Deck Deck;
    public void DrawCard(int x = 1)
    {
        for (int i = 0; i < x; i++) CurrentHand.Add(Deck.CurrentPile.DrawTop());
    }
    public void DiscardHand()
    {
        foreach (var a in CurrentHand)
        {
            Deck.DiscardPile.AddTop(a);
        }
        CurrentHand.Clear();
    }
    public bool HandContains(Card c)
    {
        foreach (var a in CurrentHand)
        {
            if (a.Matches(c.Name)) return true;
        }
        return false;
    }
    public Hand(Deck deck)
    {
        Deck = deck;
    }
}

public struct CardCollection
{
    public List<Card> Cards;

    public CardCollection(int x)
    {
        Cards = new List<Card>();
    }
    public CardCollection(CardCollection x)
    {
        Cards = new List<Card>(x.Cards);
    }
    public CardCollection(List<Card> x)
    {
        Cards = x;
    }
    public bool Contains(Card c)
    {
        foreach (var a in Cards)
        {
            if (a.Matches(c.Name)) return true;
        }
        return false;
    }
    public void AddBottom(Card c)
    {
        Cards.Insert(0, c);
    }
    public Card PeekTop()
    {
        return Cards[Cards.Count - 1];
    }
    public Card DrawTop()
    {
        var d = Cards[Cards.Count - 1];
        Cards.Remove(d);
        return d;
    }
    public void AddTop(Card c)
    {
        Cards.Add(c);
    }

    public CardCollection Shuffle()
    {
        List<Card> list = new List<Card>();
        while (Cards.Count > 0)
        {
            var x = Random.Range(0, Cards.Count);
            list.Add(Cards[x]);
            Cards.RemoveAt(x);
        }
        Cards = list;
        return this;
    }
    public void Clear()
    {
        Cards.Clear();
    }
}
