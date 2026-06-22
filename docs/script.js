(() => {
  "use strict";

  const navToggle = document.querySelector(".nav-toggle");
  const navLinks = document.querySelector(".nav-links");

  if (navToggle && navLinks) {
    navToggle.addEventListener("click", () => {
      const isOpen = navToggle.getAttribute("aria-expanded") === "true";
      navToggle.setAttribute("aria-expanded", String(!isOpen));
      navLinks.classList.toggle("is-open", !isOpen);
    });

    navLinks.addEventListener("click", (event) => {
      if (event.target.closest("a")) {
        navToggle.setAttribute("aria-expanded", "false");
        navLinks.classList.remove("is-open");
      }
    });
  }

  const copyStatus = document.getElementById("copy-status");
  const copyButtons = document.querySelectorAll("[data-copy]");

  const copyText = async (text) => {
    if (navigator.clipboard && window.isSecureContext) {
      try {
        await navigator.clipboard.writeText(text);
        return;
      } catch {
        // Some browsers expose Clipboard API support but deny permission.
        // Continue to the selection-based fallback in that case.
      }
    }

    const textarea = document.createElement("textarea");
    textarea.value = text;
    textarea.setAttribute("readonly", "");
    textarea.style.position = "fixed";
    textarea.style.opacity = "0";
    document.body.appendChild(textarea);
    textarea.select();
    const copied = document.execCommand("copy");
    textarea.remove();

    if (!copied) {
      throw new Error("Copy command was rejected.");
    }
  };

  copyButtons.forEach((button) => {
    button.addEventListener("click", async () => {
      const label = button.querySelector(".copy-label");
      const originalLabel = label ? label.textContent : "";

      try {
        await copyText(button.dataset.copy);
        button.classList.add("is-copied");
        if (label) label.textContent = "Copied";
        if (copyStatus) copyStatus.textContent = "Command copied to clipboard.";

        window.setTimeout(() => {
          button.classList.remove("is-copied");
          if (label) label.textContent = originalLabel;
        }, 1800);
      } catch {
        if (copyStatus) copyStatus.textContent = "Could not copy the command. Select the command text and copy it manually.";
      }
    });
  });

  const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  const commandText = "copyx .\\photos .\\archive\\photos";
  const typedCommand = document.getElementById("typed-command");
  const scanStage = document.getElementById("scan-stage");
  const progressStage = document.getElementById("progress-stage");
  const summaryStage = document.getElementById("summary-stage");
  const filesBar = document.getElementById("files-bar");
  const dataBar = document.getElementById("data-bar");
  const fileBar = document.getElementById("file-bar");
  const filesPercent = document.getElementById("files-percent");
  const dataPercent = document.getElementById("data-percent");
  const dataLabel = document.getElementById("data-label");
  const fileLabel = document.getElementById("file-label");
  const pauseButton = document.getElementById("terminal-pause");
  const replayButton = document.getElementById("terminal-replay");

  let runId = 0;
  let paused = false;
  let pauseStarted = 0;
  let pauseOffset = 0;
  let currentFrame = null;
  let currentTimer = null;

  const wait = (milliseconds, id) => new Promise((resolve) => {
    const check = () => {
      if (id !== runId) {
        resolve(false);
        return;
      }
      if (paused) {
        currentTimer = window.setTimeout(check, 80);
        return;
      }
      currentTimer = window.setTimeout(() => resolve(true), milliseconds);
    };
    check();
  });

  const setProgress = (value) => {
    const percentage = Math.min(100, Math.round(value));
    const filePercentage = Math.min(100, (percentage * 2.4) % 104);
    const gigabytes = (1.84 * percentage / 100).toFixed(2);
    const filenames = ["DSC_2048.jpg", "vacation-01.mp4", "raw\\sunset.cr3", "exports\\album.zip"];

    filesBar.style.width = `${percentage}%`;
    dataBar.style.width = `${percentage}%`;
    fileBar.style.width = `${filePercentage}%`;
    filesPercent.textContent = `${percentage}%`;
    dataPercent.textContent = `${percentage}%`;
    dataLabel.textContent = `Data (${gigabytes} GB / 1.84 GB)`;
    fileLabel.textContent = filenames[Math.min(filenames.length - 1, Math.floor(percentage / 26))];
  };

  const animateProgress = (duration, id) => new Promise((resolve) => {
    const startedAt = performance.now();
    pauseOffset = 0;

    const frame = (now) => {
      if (id !== runId) {
        resolve(false);
        return;
      }

      if (paused) {
        if (!pauseStarted) pauseStarted = now;
        currentFrame = requestAnimationFrame(frame);
        return;
      }

      if (pauseStarted) {
        pauseOffset += now - pauseStarted;
        pauseStarted = 0;
      }

      const elapsed = now - startedAt - pauseOffset;
      const progress = Math.min(1, elapsed / duration);
      const eased = 1 - Math.pow(1 - progress, 3);
      setProgress(eased * 100);

      if (progress < 1) {
        currentFrame = requestAnimationFrame(frame);
      } else {
        resolve(true);
      }
    };

    currentFrame = requestAnimationFrame(frame);
  });

  const resetTerminal = () => {
    if (currentFrame) cancelAnimationFrame(currentFrame);
    if (currentTimer) clearTimeout(currentTimer);
    typedCommand.textContent = "";
    scanStage.classList.remove("is-visible");
    progressStage.classList.remove("is-visible");
    summaryStage.classList.remove("is-visible");
    setProgress(0);
    pauseStarted = 0;
    pauseOffset = 0;
  };

  const showCompletedTerminal = () => {
    typedCommand.textContent = commandText;
    scanStage.classList.add("is-visible");
    progressStage.classList.add("is-visible");
    summaryStage.classList.add("is-visible");
    setProgress(100);
  };

  const runTerminal = async () => {
    runId += 1;
    const id = runId;
    paused = false;
    if (pauseButton) pauseButton.textContent = "Pause";
    resetTerminal();

    if (reducedMotion) {
      showCompletedTerminal();
      return;
    }

    for (let index = 0; index < commandText.length; index += 1) {
      if (!(await wait(32 + Math.random() * 34, id))) return;
      typedCommand.textContent = commandText.slice(0, index + 1);
    }

    if (!(await wait(480, id))) return;
    scanStage.classList.add("is-visible");
    if (!(await wait(1150, id))) return;
    progressStage.classList.add("is-visible");
    if (!(await animateProgress(4300, id))) return;
    if (!(await wait(350, id))) return;
    summaryStage.classList.add("is-visible");
  };

  if (
    typedCommand &&
    scanStage &&
    progressStage &&
    summaryStage &&
    filesBar &&
    dataBar &&
    fileBar
  ) {
    pauseButton?.addEventListener("click", () => {
      paused = !paused;
      pauseButton.textContent = paused ? "Resume" : "Pause";
    });

    replayButton?.addEventListener("click", runTerminal);
    runTerminal();
  }
})();
