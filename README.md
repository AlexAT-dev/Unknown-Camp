# Unknown Camp

Asymmetrical multiplayer game using Photon plugin for multiplayer implementation.  
ML-Agent for Deep Reinforcement Learning and Curriculum Learning.

Developed for a bachelor degree project by Oleksii Atvinovskyi (AlexAT-dev).

Server-side repository: https://github.com/AlexAT-dev/Unknown-Camp-Server

## Requirements

- Unity Editor v2022.3.50f1
- Python 3.9.13

## Python Libraries

Install with pip:

```sh
pip install mlagents torch torchvision torchaudio protobuf==3.20.3 onnx
```

- mlagents
- torch
- torchvision
- torchaudio
- protobuf==3.20.3
- onnx

## Unity & Photon

This project is built in Unity and uses [Photon Unity Networking (PUN 2)](https://doc.photonengine.com/en-us/pun/v2) for real-time multiplayer features.  
Photon provides matchmaking, room management, and reliable network synchronization for GameObjects and RPC calls.

- Photon settings are configured in the `PhotonServerSettings` asset inside the Unity Editor.
- To run multiplayer, each client must have a unique AppId (get it from [Photon Dashboard](https://dashboard.photonengine.com/)).
- The project supports both LAN and Photon Cloud connections.

### Key Features

- Asymmetrical multiplayer gameplay logic implemented with Photon.
- Networked object instantiation and synchronization using PhotonView components.
- Custom serialization for Unity types (Vector3, Quaternion, etc.) for efficient network traffic.
- ML-Agents integration for AI training and inference.
- Addressables and StreamingAssets used for content management.

## Project Structure

- `Assets/` – Unity project assets, scripts, scenes, and plugins.
- `Config/` – Configuration files.
- `ml-agents/`, `ml-agents-envs/`, `venv/` – ML-Agents and Python environment (ignored by git).
- `PhotonUnityNetworking/` – Photon plugin for multiplayer.

## Getting Started

1. Clone the repository.
2. Open the project in Unity Editor (v2022.3.50f1).
3. Install required Python libraries (see above).
4. Configure Photon App ID in `PhotonServerSettings` inside Unity.
5. To train ML-Agents, run training scripts from the project root.

## Running the Game

- Open the main scene in Unity.
- Press Play to start the game in the Editor.
- For multiplayer, build and run multiple clients or use the Unity Editor alongside a build.

## Training Agents

- Make sure ML-Agents is installed and configured.
- Use the provided training scripts or follow ML-Agents documentation to start training.

## Notes

- Photon plugin is used for real-time multiplayer networking.
- ML-Agents is used for AI agent training and inference.
- See `Assets/Photon/PhotonUnityNetworking/changelog.txt` for Photon version details.
