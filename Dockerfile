FROM squidfunk/mkdocs-material:9.7.1

# required for mkdocs-git-committers-plugin-2
RUN apk add --no-cache --virtual .build-deps gcc libc-dev libxslt-dev && \
    apk add --no-cache libxslt && \
    pip install --no-cache-dir lxml>=3.5.0 && \
    apk del .build-deps

RUN pip install --no-cache-dir \
  mkdocs-git-revision-date-localized-plugin \
  git+https://github.com/tibitoth/mkdocs-git-committers-plugin-2.git@master \
  mkdocs-glightbox

RUN git config --global --add safe.directory /github/workspace

EXPOSE 8000

ENTRYPOINT ["mkdocs", "serve", "--dev-addr=0.0.0.0:8000" ]

CMD ["--config-file=mkdocs.hu.yml"]
