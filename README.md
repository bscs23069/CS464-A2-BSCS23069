# CS464 Assignment 02 · Mayasa Fakhar · BSCS23069

- **Video:** [VIDEO LINK]
- **Input:** InputSystem.actions, the Move and Jump actions from the project-wide actions asset (Active Input Handling is set to Input System Package)
- **Ground check:** contact normal, a contact only counts if `Vector3.Dot(normal, Vector3.up) > 0.5`
- **Camera:** SmoothDamp with look-ahead from the player's velocity
- **Juice:** a squash on Body when the player lands, eased with Ease.OutBack, started on a landing after more than 0.1 s in the air

## Tuning

| Setting | Value | Why this number |
|---|---|---|
| Speed | 6 m/s | Each level is about 69 m from spawn to goal, so a straight run takes about 11.5 s and all five fit in a 60 s video. It also makes one jump carry 6.1 m, more than my widest gap. |
| Jump height | 1.8 m | My ledges are 1.2 m, so this leaves 0.6 m spare to get the feet on top. The 2 m blocks in Level02 were too tall for it, so I lowered them. |
| Time to apex | 0.61 s | It follows from the height: v0 = √(2 · 9.81 · 1.8) = 5.94 m/s, and 5.94 / 9.81 = 0.61 s. |
| Fall multiplier | 2.2× | The fall takes 0.61 / √2.2 = 0.41 s instead of 0.61 s, so the jump lasts 1.01 s and doesn't float. The apex stays at 1.8 m. |
| Coyote time | 0.1 s | At 6 m/s that is 0.6 m past the edge, which is enough to catch a late press at the gaps in Level02 and on the bridge in Level04. |
| Jump buffer | 0.12 s | At the end of a fall that covers about the last 0.9 m, so pressing a bit before landing on a ledge still jumps straight away. |
| Camera smoothing | smoothTime 0.3 s | At 6 m/s SmoothDamp trails about 6 × 0.3 = 1.8 m behind. It's smooth on landings but still keeps up in the hall. |
| Look-ahead | 0.35 s | At 6 m/s the aim point moves 2.1 m ahead, a bit more than the 1.8 m lag, so I can see a gap before I reach it. |

## Four answers

1. **The jump, in numbers.** With g = 9.81 and jumpHeight = 1.8 m, the launch speed is v0 = √(2 · 9.81 · 1.8) = 5.94 m/s. The player's mass is 1 kg, so the impulse (jumpForce) is m · v0 = 5.94 N·s. It takes 0.61 s to reach the top. With 2.2× gravity on the way down, the fall from 1.8 m takes √(2 · 1.8 / (9.81 · 2.2)) = 0.41 s, so the whole jump is about 1.01 s. At 6 m/s that carries me 6.1 m on flat ground. My widest gap is 3.85 m in Level02 (from the last 1.2 m platform down into the goal room), so it clears it with more than 2 m to spare. In the game the apex comes out at about 1.74 m because physics runs in 0.02 s steps.
2. **Update and FixedUpdate.** Update reads the Move and Jump actions, builds the move direction from the camera, stamps the coyote and buffer times, queues the jump when both windows are open, and starts the squash on a landing. FixedUpdate sets `rb.linearVelocity` from the move, applies the queued jump once as an impulse, adds the fall gravity and resets isGrounded. If I swapped them, reading `WasPressedThisFrame` in FixedUpdate would miss presses, because at 144 fps about two frames in three have no FixedUpdate. Setting the velocity and adding forces in Update would also run a different number of times at different frame rates, so the jump and the fall would change with the frame rate.
3. **Vector3.Dot.** It is in my ground check: `if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)`. Both vectors are length 1, so the dot is the cosine of the angle between the surface normal and up. A floor gives 1, a wall gives 0 and a ceiling gives -1. A positive dot means the surface faces up, and above 0.5 means it's less than 60° from flat, so only that counts as ground. That's why I can't jump again while touching a wall.
4. **The juice.** It's a squash on landing. Body goes to y = 0.7 and x, z = 1 / √0.7 = 1.2 so the volume stays the same, then Ease.OutBack brings it back to 1 over 0.25 s, which is 15 frames at 60 fps. OutBack overshoots to about 1.1, so Body stretches to y = 1.03 for a moment before settling, and it reads like a bounce. I use LerpUnclamped so the overshoot isn't cut off, and set the scale to exactly 1 at the end. 0.25 s is short enough that it's done before the next jump. It only starts after more than 0.1 s in the air, and a running squash is stopped first.

## Levels

| Level | What the controller must do there | What I changed for this jump |
|---|---|---|
| Level01 | Run 69 m down the hall from the spawn room to the goal room. There are no jumps. | Took the collider off the leading lines prefab. The lines are 0.4 m off the floor right on the route, so the capsule used to catch on them. |
| Level02 | Jump up onto a 1.2 m platform, jump a 3.6 m gap down to a 0.5 m platform, a 2.8 m gap back up to 1.2 m, a 3.85 m gap down into the goal room, then climb the 1.2 m step to the goal. | Lowered the two 2 m platforms and the goal step to 1.2 m, because a 1.8 m jump can't get the feet on top of 2 m. I kept the gaps since the jump carries 6.1 m. |
| Level03 | Go left or right around the big block in the middle, then weave past the smaller blocks and the cylinder to the goal room. | Made the coin prefab a trigger. The coins were solid spheres on the route, so the capsule bumped into them. |
| Level04 | Climb three 1.2 m ledges up to 3.6 m, run along a 1.5 m wide bridge and jump the 2.5 m gap in it, then drop down two ledges to the goal. | New level for this jump. The ledges are 1.2 m and the gap is 2.5 m to suit it. |
| Level05 | Run through a 3 m wide corridor into a yard, then hop over or go round the 1.1 m low cover to reach the goal. | New level. The low cover is 1.1 m so the jump clears it easily. |
