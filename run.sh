#!/bin/sh
tmux new-session \; \
  send-keys '#Switch = control+b arrows' C-m \; \
  send-keys '#Split = control+b %' C-m \; \
  send-keys '#Kill = control+b x' C-m \; \
  send-keys 'cd Client && dotnet run -c Release' C-m \; \
  split-window -h -p 50 \; \
  send-keys '#Switch = control+b arrows' C-m \; \
  send-keys '#Split = control+b %' C-m \; \
  send-keys '#Kill = control+b x' C-m \; \
  send-keys 'cd Server && dotnet run -c Release' C-m \; \
  select-pane -t0 \;  