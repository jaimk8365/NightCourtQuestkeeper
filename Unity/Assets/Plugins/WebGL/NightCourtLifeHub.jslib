mergeInto(LibraryManager.library, {
  NC_RequestQuestKeys: function () {
    var records = [];
    try {
      var save = JSON.parse(localStorage.getItem('ncg_save_v1') || '{}');
      (save.quests || []).forEach(function (q) {
        if (!q || !q.id || !q.title || q.locked) return;
        records.push({ Id:q.externalRef || q.id, Title:q.title, Type:q.type || 'micro', Minutes:q.minutes || 5, Realm:q.realm || 'admin', CompletedOn:q.completedOn || null });
      });
      var inbox = JSON.parse(localStorage.getItem('lifehub_quest_inbox_v1') || '[]');
      (inbox || []).forEach(function (q) {
        if (!q || !q.id || !q.title) return;
        records.push({ Id:q.id, Title:q.title, Type:(q.minutes || 10) <= 5 ? 'micro' : 'focus', Minutes:q.minutes || 10, Realm:q.realm || 'admin', CompletedOn:null });
      });
    } catch (e) {}
    SendMessage('LifeHubBridge', 'ReceiveLifeHubQuests', JSON.stringify({ Quests: records }));
  },
  NC_SaveQuestCompletion: function (jsonPtr) {
    try {
      var record = JSON.parse(UTF8ToString(jsonPtr));
      var save = JSON.parse(localStorage.getItem('ncg_save_v1') || '{}');
      var changed = false;
      (save.quests || []).forEach(function (q) {
        if ((q.externalRef || q.id) === record.Id && !q.completedOn) { q.completedOn = record.CompletedOn; changed = true; }
      });
      if (changed) localStorage.setItem('ncg_save_v1', JSON.stringify(save));
    } catch (e) {}
  }
});
