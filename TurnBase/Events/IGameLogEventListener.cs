using System.Collections.Generic;

namespace TurnBase
{
    public interface IGameLogEventListener<TInitResponseModel, TMoveResponseModel, TMoveNotificationModel>
    {
        void GameLogStarted(IField field);
        void GameLogPlayerInitialized(int playerNumber, InitResponseModel<TInitResponseModel> initResponseModel, IField field);
        void GameLogPlayerDisconnected(int playerNumber, IField field);
        void GameLogTurnFinished(IField field);
        void GameLogPlayerWrongTurn(int playerNumber, MoveValidationStatus status, TMoveResponseModel moveResponseModel, IField field);
        void GameLogPlayerTurn(int playerNumber, TMoveNotificationModel moveNotificationModel, TMoveResponseModel moveResponseModel, IField field);
        void GameLogFinished(List<int> winners, IField field);
    }
}