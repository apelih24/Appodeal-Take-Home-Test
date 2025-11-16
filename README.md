# Appodeal-Take-Home-Test

## What I have built
A simple Solitaire gameplay:
- Dynamic stacks spawning
- Dynamic cards spawning
- An ability to drag and drop cards between stacks

## What I have improved if I had more time
- Spent more time for internet searching and best practices for Solitaire gameplay implementation.
- Currently, MoveRecord has reference to the card gameObject, it will be a problem if undo feature must work between sessions (app relaunch), so I would probably rewrite that part to have cardId or have some other way to restore a card gameObject.
- I think that it would be better to have Command pattern implementation for each action that user can do (move card, draw card, etc) instead of saving a state (MoveRecord -> MoveCommand).
- Manage memory better. Currently,there are no unsubscriptions for events, which can lead to memory leaks on scene change.
- Migrate game from UI to Sprites. UI was picked for faster prototyping, but Sprites would be more suitable for final product.
- Add R3 for UI reactive synchronization with game state.
- Implement default Solitaire rules (foundations, suits, etc). Add sprites for cards. (But this was out of scope for the test).

## Which parts was AI assisted and how I have prompted it
- GitLab Copilot was used throw codding to suggest code snippets and implementations.
- ChatGPT Codex model was used to generate initial boilerplate code. Prompts were focused on the task description, needed result and my vision of the implementation. After that I've iterated a few times to get a better result. After that some manual changes were made, because I was not fully satisfied with the result.
- At last ChatGPT Codex model was used at the end to validate that all requirements are met after my manual changes.
