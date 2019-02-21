﻿using Moq;
using Moq.Protected;
using NUnit.Framework;
using OpenrecYoyakuPlugin;
using Plugin;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using YouTubeLiveSitePlugin;

namespace YoyakuPluginTests
{
    [TestFixture]
    public class コマンドテスト
    {
        class TestHost : IPluginHost
        {
            public string SettingsDirPath { get; } = "";
            public double MainViewLeft { get; } = 0;
            public double MainViewTop { get; } = 0;
            public bool IsTopmost { get; } = false;

            public IEnumerable<IConnectionStatus> GetAllConnectionStatus()
            {
                throw new NotImplementedException();
            }

            public string LoadOptions(string path)
            {
                var options = new DynamicOptions()
                {
                    IsEnabled = true,
                };
                return options.Serialize();
            }

            public void PostComment(string guid, string comment)
            {
            }

            public void PostCommentToAll(string comment)
            {
            }

            public void SaveOptions(string path, string s)
            {
            }
        }
        PluginBody _plugin;
        IMessageMetadata _messageMetadata;
        SettingsViewModel _vm;
        [SetUp]
        public void Setup()
        {
            var host = new TestHost();
            var options = new DynamicOptions()
            {
                IsEnabled = true,
            };

            var vmMock = new Mock<SettingsViewModel>(host, options, Dispatcher.CurrentDispatcher);
            vmMock.Protected().Setup<DateTime>("GetCurrentDateTime").Returns(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime());
            var vm = vmMock.Object;
            _vm = vm;
            //vm.on
            var messageMetadataMock = new Mock<IMessageMetadata>();
            var pluginMock = new Mock<PluginBody>() { CallBase = true };
            pluginMock.Protected().Setup<SettingsViewModel>("CreateSettingsViewModel").Returns(vm);
            pluginMock.Protected().Setup<IOptions>("LoadOptions").Returns(options);


            var messageMetadata = messageMetadataMock.Object;
            _messageMetadata = messageMetadata;

            var plugin = pluginMock.Object;
            _plugin = plugin;
            plugin.Host = host;
            plugin.OnLoaded();


        }
        class YtComment : IYouTubeLiveComment
        {
            public YouTubeLiveMessageType YouTubeLiveMessageType { get; } = YouTubeLiveMessageType.Comment;
            public string Id { get; set; }
            public string UserId { get; set; }
            public string PostTime { get; set; }
            public IMessageImage UserIcon { get; set; }
            public string Raw { get; set; }
            public SiteType SiteType { get; } = SiteType.YouTubeLive;
            public IEnumerable<IMessagePart> NameItems { get; set; }
            public IEnumerable<IMessagePart> CommentItems { get; set; }

            public event EventHandler<ValueChangedEventArgs> ValueChanged;
        }
        [Test]
        public void 登録が可能か()
        {
            AddComment("/yoyaku", "1");
            var ms = _vm.RegisteredUsers.ToArray();
            Assert.AreEqual(new[]{
                new User{  Id="1" },
                }, ms);
        }
        [Test]
        public void 複数人が登録可能か()
        {
            AddComment("/yoyaku", "1");
            AddComment("/yoyaku", "2");
            var ms = _vm.RegisteredUsers.ToArray();
            Assert.AreEqual(new[]{
                new User{  Id="1" },
                new User{  Id="2" },
                }, ms);
        }
        [Test]
        public void 登録済みの場合に重複登録されないか()
        {
            AddComment("/yoyaku", "1");
            AddComment("/yoyaku", "1");
            var ms = _vm.RegisteredUsers.ToArray();
            Assert.AreEqual(new[]{
                new User{  Id="1" },
                }, ms);
        }
        [Test]
        public void 取り消しが正常に動作するか()
        {
            AddComment("/yoyaku", "1");
            AddComment("/yoyaku", "2");
            AddComment("/torikeshi", "1");
            var ms = _vm.RegisteredUsers.ToArray();
            Assert.AreEqual(new[]{
                new User{  Id="2" },
                }, ms);
        }
        private void AddComment(string comment, string userId)
        {
            var message = new YtComment()
            {
                NameItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText("name") },
                CommentItems = new List<IMessagePart> { Common.MessagePartFactory.CreateMessageText(comment) },
                UserId = userId,
            };
            _plugin.OnMessageReceived(message, _messageMetadata);
        }
        [TearDown]
        public void TearDown()
        {
        }
    }
}
