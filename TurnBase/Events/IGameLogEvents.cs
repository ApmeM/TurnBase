using System;
using System.Collections.Generic;

namespace TurnBase
{
    public interface IGameLogEvents<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel>
    {
        event Action<IField> GameLogStarted;
        event Action<int, InitResponseModel<TInitResponseModel>, IField> GameLogPlayerInitialized;
        event Action<int, IField> GameLogPlayerDisconnected;
        event Action<IField> GameLogTurnFinished;
        event Action<int, MoveValidationStatus, TMoveResponseModel, IField> GameLogPlayerWrongTurn;
        event Action<int, TMoveNotificationModel, TMoveResponseModel, IField> GameLogPlayerTurn;
        event Action<List<int>, IField> GameLogFinished;
    }
}