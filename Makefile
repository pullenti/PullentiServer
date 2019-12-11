
VERSION = 3.20.2
IMAGE = pullenti/pullenti-server

../PullentiNetCore:
	git clone https://github.com/pullenti/PullentiNetCore.git ../PullentiNetCore

EP.SdkCore: ../PullentiNetCore
	cp -r ../PullentiNetCore/EP.SdkCore .

image: EP.SdkCore
	docker build -t $(IMAGE):$(VERSION) .
	docker tag $(IMAGE):$(VERSION) $(IMAGE)

push:
	docker push $(IMAGE)
	docker push $(IMAGE):$(TAG)

up:
	docker run -d --name pullenti -p 8080:8080 $(IMAGE)

down:
	docker rm -f pullenti

test:
	python test.py
