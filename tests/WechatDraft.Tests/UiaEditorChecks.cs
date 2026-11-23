using WechatDraft.Windows;

internal static class UiaEditorChecks
{
    private static WechatAdapter.ChatEditorProbe Probe(string automationId, double relativeLeft, double relativeTop, double widthDip) =>
        new(automationId, relativeLeft, relativeTop, widthDip);

    public static IEnumerable<(string Name, Func<Task> Run)> Cases()
    {
        yield return ("Detached chat window accepts its only input left of twenty percent of the width", () =>
        {
            // Measured on a 503x685 logical window: the input starts at 11.7% and the editor at 13.3%
            // because only the icon rail precedes the chat. The old rule required more than 20%.
            var selected = WechatAdapter.SelectChatEditors([Probe("", .117, .79, 436)], out var candidates);
            if (candidates != 1) throw new Exception("Detached input was not counted as a candidate.");
            if (selected.Count != 1 || selected[0] != 0) throw new Exception("The only chat input of a detached window was rejected.");
            return Task.CompletedTask;
        });
        yield return ("A maximised detached window keeps an input flush with the window edge", () =>
        {
            // Measured on a 1194x1280 physical window at 200 percent: the single Edit control starts at
            // 1.6% of the width. Any left-edge floor, including the earlier 6%, rejected it.
            var selected = WechatAdapter.SelectChatEditors([Probe("", .016, .77, 575)], out var candidates);
            if (candidates != 1 || selected.Count != 1 || selected[0] != 0)
                throw new Exception("A chat input flush with the window edge was rejected.");
            return Task.CompletedTask;
        });
        yield return ("WeChat's own editor identifier outranks the left-edge heuristic", () =>
        {
            var selected = WechatAdapter.SelectChatEditors([Probe("", .25, .79, 300), Probe("chat_input_field", .117, .79, 436)], out var candidates);
            if (candidates != 2) throw new Exception("Named and positional candidates were not both counted.");
            if (selected.Count != 1 || selected[0] != 1) throw new Exception("The named chat input did not outrank the positional one.");
            return Task.CompletedTask;
        });
        yield return ("A wide read-write box above the message pane is never treated as the chat input", () =>
        {
            var selected = WechatAdapter.SelectChatEditors([Probe("", .30, .10, 400), Probe("", .90, .30, 240)], out var candidates);
            if (candidates != 0 || selected.Count != 0) throw new Exception("An upper or narrow edit control was accepted as the chat input.");
            return Task.CompletedTask;
        });
        yield return ("Two positional candidates stay ambiguous instead of picking one input", () =>
        {
            var selected = WechatAdapter.SelectChatEditors([Probe("", .25, .79, 300), Probe("", .30, .60, 320)], out var candidates);
            if (candidates != 2 || selected.Count != 2) throw new Exception("Ambiguous inputs were silently reduced to one.");
            return Task.CompletedTask;
        });
        yield return ("A single unnamed candidate left of twenty percent is still accepted", () =>
        {
            var selected = WechatAdapter.SelectChatEditors([Probe("", .117, .79, 436), Probe("", .05, .05, 120)], out var candidates);
            if (candidates != 1 || selected.Count != 1 || selected[0] != 0)
                throw new Exception("A lone candidate was dropped because other edit controls failed the basic filters.");
            return Task.CompletedTask;
        });
    }
}
