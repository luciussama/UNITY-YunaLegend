using NUnit.Framework;
using YunaLegend.Qa.Core;

namespace YunaLegend.Qa.Tests
{
    public sealed class DarkRoomQaPilotTests
    {
        [Test]
        public void ShouldJump_WhenApproachingForwardSupportEdge_ReturnsTrue()
        {
            bool jump = DarkRoomQaPilot.ShouldJump(
                playerX: 4.75f,
                runDirection: 1f,
                runSpeed: 5.2f,
                isGrounded: true,
                hasCurrentSupportEdge: true,
                currentSupportForwardEdgeX: 5.0f,
                hasNextPlatform: true,
                nextPlatformNearEdgeX: 7.2f);

            Assert.IsTrue(jump);
        }

        [Test]
        public void ShouldJump_WhenAirborne_ReturnsFalse()
        {
            bool jump = DarkRoomQaPilot.ShouldJump(
                playerX: 4.85f,
                runDirection: 1f,
                runSpeed: 5.2f,
                isGrounded: false,
                hasCurrentSupportEdge: true,
                currentSupportForwardEdgeX: 5.0f,
                hasNextPlatform: true,
                nextPlatformNearEdgeX: 7.2f);

            Assert.IsFalse(jump);
        }

        [Test]
        public void ShouldJump_OnReturnDirectionNearEdge_ReturnsTrue()
        {
            bool jump = DarkRoomQaPilot.ShouldJump(
                playerX: 10.2f,
                runDirection: -1f,
                runSpeed: 5.2f,
                isGrounded: true,
                hasCurrentSupportEdge: true,
                currentSupportForwardEdgeX: 10.0f,
                hasNextPlatform: true,
                nextPlatformNearEdgeX: 7.5f);

            Assert.IsTrue(jump);
        }
    }
}
