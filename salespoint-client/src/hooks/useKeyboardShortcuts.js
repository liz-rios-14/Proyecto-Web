import { useEffect } from "react";

export default function useKeyboardShortcuts(shortcuts, dependencies = []) {
  useEffect(() => {
    const handleKeyDown = (event) => {
      const key = event.key.toLowerCase();

      shortcuts.forEach((shortcut) => {
        const isCtrlValid = !!shortcut.ctrl === event.ctrlKey;
        const isAltValid = !!shortcut.alt === event.altKey;
        const isShiftValid = !!shortcut.shift === event.shiftKey;
        const isKeyValid = shortcut.key.toLowerCase() === key;

        if (isCtrlValid && isAltValid && isShiftValid && isKeyValid) {
          event.preventDefault();
          shortcut.action();
        }
      });
    };

    window.addEventListener("keydown", handleKeyDown);

    return () => {
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, dependencies);
}