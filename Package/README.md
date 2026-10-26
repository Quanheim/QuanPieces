# QuanPieces

New building pieces for the things the vanilla build menu can't quite do. Each one exists because it solves a
real building problem, and they all live in their own **QuanPieces** tab on the hammer.

Everyone on a server needs the mod installed: the server and every player connecting to it.

## Pieces

<details>
<summary><b>Quan's Chimney Flue</b></summary>

<p><img src="https://raw.githubusercontent.com/Quanheim/QuanPieces/main/docs/images/chimney-flue.jpg" alt="Quan's Chimney Flue seen from below, its stone louvres above a lit campfire inside a wooden chimney" width="480"></p>

<p><img src="https://raw.githubusercontent.com/Quanheim/QuanPieces/main/docs/images/chimney-flue-tooltip.png" alt="The flue's build-menu card: Blocks rain but not smoke. Must be 1.5m or more above a fireplace. For Valyyn. Costs 5 Stone at a Workbench." width="480"></p>

A 2x2 chimney flue that sits above your fireplace, inside your chimney, and stops the rain putting out your
fire whilst still letting the smoke escape. No longer do you need a roof over your chimneys!

| Cost | Crafting station | Size |
|---|---|---|
| 5 Stone | Workbench | 2x2, snaps and tiles with itself to make larger flues |

> **Place it at least 1.5 m above your fire**, so the smoke can form without putting the fire out.

- It only needs to cover the space directly above the fire. For wider chimneys, snap several together to
  fill the opening.
- It doesn't change how smoke behaves: a chimney that backs smoke up without the flue still will with it.
- You can walk through it. Only its edges are solid.

</details>

<details>
<summary><b>Quan's Street Lights</b></summary>

<table>
<tr>
<td><img src="https://raw.githubusercontent.com/Quanheim/QuanPieces/main/docs/images/streetlight-brazier.jpg" alt="Quan's Brazier Street Light: a coal brazier hung from a wooden post and arm" width="320"></td>
<td><img src="https://raw.githubusercontent.com/Quanheim/QuanPieces/main/docs/images/streetlight-lantern.jpg" alt="Quan's Dvergr Street Light: a Dvergr lantern hung from a Grausten and Ashwood post with a carved arch" width="320"></td>
</tr>
<tr>
<td><img src="https://raw.githubusercontent.com/Quanheim/QuanPieces/main/docs/images/streetlight-brazier-card.png" alt="Build-menu card: 5 Bronze, 2 Coal, 1 Chain and 4 Wood at a Forge" width="320"></td>
<td><img src="https://raw.githubusercontent.com/Quanheim/QuanPieces/main/docs/images/streetlight-lantern-card.png" alt="Build-menu card: 3 Copper, 1 Dvergr Lantern, 1 Chain, 6 Grausten and 4 Ashwood at a Black Forge" width="320"></td>
</tr>
</table>

Two street lights for lighting paths, roads and the spaces between buildings: a coal brazier for the middle of
the game and a Dvergr lantern for later on. Both work like any other fire, so you refuel them by hand, and with
[NoSmokeStayLit](https://thunderstore.io/c/valheim/p/TastyChickenLegs/NoSmokeStayLit/) you can put them on its
day/night timer or keep them lit.

| | Brazier Street Light | Dvergr Street Light |
|---|---|---|
| **Cost** | 5 Bronze, 2 Coal, 1 Chain, 4 Wood | 3 Copper, 1 Dvergr Lantern, 1 Chain, 6 Grausten, 4 Ashwood |
| **Crafting station** | Forge | Black Forge |
| **Fuel** | Coal | Wisps |
| **Rain** | Stays lit, sheltered by its arm | Stays lit |
| **Strong wind** | Burns low, like any open fire | Stays lit, it's enclosed |
| **Light** | A little brighter and wider than a hanging brazier | A wide pool: lamps about 30 m apart light the whole path |
| **Warmth** | Up to 8 m away, like a hanging brazier | Up to 50 m away, so whole paths stay warm |

- Both stand on the ground or on floors, and need the same support as a wooden post.
- They're 4 m tall, with the light hanging 2.5 to 3 m up.
- To use them with NoSmokeStayLit's timer, add `Quan_streetlight_brazier` and `Quan_streetlight_lantern` to its
  **Custom Items on Timers** setting.

</details>
