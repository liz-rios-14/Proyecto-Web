export default function KeyboardHelp({ title = "Atajos", shortcuts = [] }) {
  if (shortcuts.length === 0) return null;

  return (
    <div className="keyboard-help">
      <p className="keyboard-help-title">{title}</p>

      <div className="keyboard-help-list">
        {shortcuts.map((shortcut) => {
          const [keys, description] = shortcut
            .split("→")
            .map((part) => part.trim());

          return (
            <div className="keyboard-help-item" key={shortcut}>
              <kbd>{keys}</kbd>
              {description && <span>{description}</span>}
            </div>
          );
        })}
      </div>
    </div>
  );
}
