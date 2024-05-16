# Nemesis STG Engine: How to use
This engine constructs its nodes on-the-fly using just scripts and resources, and uses an object pool. A lot of behavior is handled by the engine itself, simplifying your work.
> Note: Despite the amount of things that can simply be done without code, this engine does expect you to do some programming. However, some additional friendliness to non-programmers is underway.

> Note: A common way to set up collisions is to have one collision layer for the world, another for players, and another for enemies.

## How to create a stage
1. Create a new scene with an STGController node; it is a Node2D.
1. Set the node's stageMovement variable to determine the direction the stage should be moving.
1. Add any objects and scene-related items as a child of this node. Special nodes can be found in the [objects](#objects) section. You may also be interested in the [player creation](#creating-a-player) and [entity creation](#creating-an-entity) section.
1. Save the scene as a file.
1. Optionally, you may create and save another scene with a ParallaxBackground and its layers to be spawned later with the scene.

## How to load a stage
1. Create a new function where your loading will be done. This can be either C# or GDScript.
    > Note: If you are using GDScript, you will have to load the STGController script from disk using `preload`. This way you can access static functions.
1. Load your new scene(s) from disk via `preload` (other methods work too, so long as it's acquired), but do not instantiate it.
1. Locate the node you want the scene to spawn on. Preferably you can just use `this`/`self`.
1. Call `STGController.LoadStage` with your stage scene and the node, and optionally what it should do (ClearStats is the default). This will return the new instance.
    > Note: If you have a parallax background scene, you can instantiate it and set the stage instance's `parallaxBgPath` to its path in the current scene tree.
1. The instance now also exists in `STGController.Instance` and can be unloaded with `STGController.UnloadStage` (and reloaded with `STGController.ReloadStage`, utilizing optional behavior; UseCheckpoint is the default).

## Creating an entity
> Entities are simply objects in the game world that are normally removed when they move out of sight. This can be like a projectile or non-playable ship.
1. In a folder, create an EntityResource.
1. Scrolling down to the "SpawnResource" section, we can set its:
    - Name
    - Sprite sequence (if you want it to be not animated, you can just put one frame with your desired texture and nothing else)
    - Whether or not the texture stays fixed (won't rotate with the entity)
    - Script override, in case we want the entity to have special behavior
    - Sounds
    - Collision shape (can be copied over from a dummy CollisionShape2D)
    - Collision layer and mask (same as above; you may have to right-click these variables in the inspector to do so)
    - SpawnResource that is spawned when this entity despawns (useful for spawning special FX)
    - SpawnResource that is spawned on an interval. This is usually where bullets are spawned from ships.
1. In the "EntityResource" section, we can set its:
    - Motion type ("Standard" means it will move in a straight line)
    - Path that it follows, if the motion type is set to "Path"
    - Option to loop the above path
    - Follow a given node that exists or will be created (SpawnResources spawn and set their name using the "Name" variable), if the motion type is set to "follow"
    - Speed
    - Turn speed (turns constantly in standard, will turn towards what it's supposed to be following if set to "follow")
    - Ability to move with its spawner
    - Time until it should move with the stage (0 to never do this)
    - Collision damage
    - Miscellaneous/world collision damage
    - HP (can be 0)
    - Score (only applies when destroyed by the player)
    - Identity as a boss (can be used to identify minibosses too)
    - Ends the stage on despawn/destruction
    - What should be spawned when it takes damage and HP reaches 0
1. If you want multiple of these to spawn or want them to appear in the stage, a spawner can be created. See [creating a spawner](#creating-a-spawner) for more info.

## Creating a player
> Players are entities with player control attached to them. They cannot escape the screen and can use its interval on-command.
1. Follow the steps in [entity creation](#creating-an-entity), but instead of an EntityResource, create a PlayerResource. Withhold for now on spawner creation.
1. In the "PlayerResource" section, we can set its:
    - Device ID (not needed if keyboard-controlled)
    - Ability to be listened to via keyboard
1. When [creating a spawner](#creating-a-spawner), set the trigger to PlayerSpawnEvent and despawn condition to "None." It will automatically move with the scene, so it will stay in the same place on-screen.

## Creating a spawner
> Spawners are what place entities in the world. They can spawn multiple objects and can be triggered via different ways.
1. In a folder, create a SpawnerDataResource.
1. Though the "SpawnResource" section can be ignored, we can set its:
    - Name
    - Sprite sequence (it might not play out as you expect, considering the spawner behaves and disables itself independently)
    - Whether or not the texture stays fixed (won't rotate with the entity)
    - Script override, in case we want the entity to have special behavior
    - Sounds
    - Collision shape (can be copied over from a dummy CollisionShape2D)
    - Collision layer and mask (same as above; you may have to right-click these variables in the inspector to do so)
    - SpawnResource that is spawned when this item despawns (useful for spawning special FX)
    - SpawnResource that is spawned on an interval. Not recommended to be used for this type of resource.
1. In the "SpawnerDataResource" section, we can set its:
    - Trigger condition ("OnEvent" will listen to the _OnEvent function, which can be hooked up to any signal. "OnPlaced" is when the spawner is either spawned or is ready in a scene)
    - SpawnResource to spawn
    - Spawn points (multiple can be set in the same place)
    - Starting rotation of all objects
    - Rotation increment per spawn point
    - Ability to move with its spawner
    - Time to spawn all objects (0 is instant)
    - Despawn condition
1. If this will be placed in the world, create a Spawner node in your scene and set its spawn data to the resource you just created.

## Objects
- Resources
    - EntityResource: Data for bullets and other entities. SpawnResource.
    - EntityPhasedResource: Entity data with phases.
    - EntityPhase: Data that specifically defines one phase.
    - PlayerResource: Entity data specifically for players.
    - PickupResource: Data for pickup objects. SpawnResource.
    - WeaponResource: Data provided to all PickupResources and EntityPhases that overrides the usual interval spawn.
    - SpawnerDataResource: Data used for spawners. SpawnResource.
    - SpecialFXResource: Data for special particle effects. SpawnResource.
    - SpawnableSounds: Sound data provided to all SpawnResources.
- Nodes
    - StageTrigger: A node that sends out a signal when a condition is met, with various condition options and prebuilt behaviors.
    - Spawner: A node that spawns other nodes that inherit from Spawnable. They utilize SpawnerDataResource for their `spawnData`, and will instantiate a new Spawnable node from the data's `spawn` variable on every spawn point defined in `spawnPoints`. Spawnable.
    - SpecialFXNode: A particle effect node that uses SpecialFXResource data from `fxResource`. Triggers on spawned. Spawnable.
    - Entity: An entity that moves around in the world, based on an EntityResource (or extending resource) provided to `entityData`. Spawners are a better approach as entities will even perform their behavior off-screen. Despawned when seen once and exits the player's view. Spawnable.
    - PlayerEntity: Same as above, but requires a PlayerResource instead of an EntityResource. Stays within screen bounds.
    - Pickup: A pickup item that the player can collect. Despawned when seen once and exits the player's view. Spawnable.

## Named sprite animations
> There are predefined names for animations that will trigger based on certain events. Naming your sequence this will cause it to be played based on its event.
- "spawn": Plays on spawn, and the collision will wait until it is complete before being enabled.
- "idle"/"default": Plays on idle.
- "destroy": Plays when the entity's health reaches zero from damage. Will not despawn until the animation is complete.