
VERSION = 4.1
IMAGE = pullenti/pullenti-server

../PullentiCSharp:
	git clone https://github.com/pullenti/PullentiCSharp.git ../PullentiCSharp

Pullenti: ../PullentiCSharp
	cp -r ../PullentiCSharp/PullentiCSharp/Pullenti .

image: Pullenti
	docker build -t $(IMAGE):$(VERSION) .
	docker tag $(IMAGE):$(VERSION) $(IMAGE)

push:
	docker push $(IMAGE)
	docker push $(IMAGE):$(VERSION)

up:
	docker run -d --name pullenti -p 8080:8080 $(IMAGE)

down:
	docker rm -f pullenti

test:
	python test.py
