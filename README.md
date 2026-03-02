# Cool Card Game

A video game made with Unity Engine and C#, meant to be a simple project of what we can achieve as a team and a test of my programming skills.

BEWARE: This project wasn't meant to be taken this far, but it was well recieved and went through a lot of systematic changes in the code. That's why there may be some "identity crisis" with some managers and quite a lot of rushed code, yet right now it just works.
# About
**Original Creator**: Trealius Fang

**Main Illustrator/Artist**: Dilapu

*Also thanks to GGT-Team*

**Estimated project duration**: 2026-2027


It is a card game that includes a wide range of fantastical-cultural *(and maybe in near future, myhtological)* characters. Almost all cards have their own unique **passives, stats and preferred targets** which is the reason why this game is this much chaotic.

**Currently the main systems are:** Passive, Status Effects, Target System and the stat types.

# Passive System:
Has unique call times:
| Passive Timings  | Explanation                             | 
| ---------------- | --------------------------------------- | 
| **PutOnPlay**    | As soon as the card is put on the playing field | 
| **StartOfTurn**  | When it is the cards turn to play       |
| **EndOfTurn**    | When it is the end of the cards turn to play   | 
| **OnAttack**     | When landing a *successful* attack to a card           | 
| **OnHurt**       | When taking damage by another card                  | 
| **OnHeal**       | When healing another card             | 
| **OnCured**      | When getting healed by a card       | 
| **OnDeath**      | When Resistance Points reaches 0 or less, runs just before Die() function, so it has a chance to revive itself        |
| **StartOfRound** | When round starts       | 
| **EndOfRound**   | When round ends        | 

...More explanation for later
