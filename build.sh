#!/bin/sh
tmux new-session \; \
  send-keys '#control+b arrows' C-m \; \
  send-keys 'cd Client && dotnet run' C-m \; \
  split-window -h \; \
  send-keys '#control+b arrows' C-m \; \
  send-keys 'cd Server && dotnet run' C-m \; \
  select-pane -t0 \;